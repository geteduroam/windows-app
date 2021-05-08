using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Device.Location;
using System.Globalization;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace EduroamConfigure
{
	public class IdentityProviderDownloader : IDisposable
	{
		// constants
		private static readonly Uri GeoApiUrl = new Uri("https://geo.geteduroam.app/geoip");
#if DEBUG
		private static readonly Uri ProviderApiUrl = new Uri("https://discovery.eduroam.app/v1/discovery.json");
#else
		private static readonly Uri ProviderApiUrl = new Uri("https://discovery.eduroam.app/v1/discovery.json");
#endif

		// http objects
		private static HttpClientHandler handler;
		private static HttpClient client;

		// state
		public List<IdentityProvider> Providers { get; private set; }
		public List<IdentityProvider> ClosestProviders { get; private set; } // Providers presorted by geo distance
		private GeoCoordinateWatcher GeoWatcher { get; }
		public bool Online { get => Providers != null; }
		// Variable to prevent calling loadProviders when it's already running
		private Task LoadProvidersLock = null;

		static IdentityProviderDownloader() {
			handler = new HttpClientHandler();
			handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | DecompressionMethods.Deflate;
			handler.AllowAutoRedirect = true;
			client = new HttpClient(handler);
#if DEBUG
			client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + "+DEBUG HttpClient (Windows NT 10.0; Win64; x64)");
#else
			client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + " HttpClient (Windows NT 10.0; Win64; x64)");
#endif
			client.Timeout = new TimeSpan(0, 0, 3);
		}

		/// <summary>
		/// The constructor for this class.
		/// Will download the list of all providers
		/// </summary>
		public IdentityProviderDownloader()
		{
			GeoWatcher = new GeoCoordinateWatcher();
			GeoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
			Task.Run(GetClosestProviders);
		}

		/// <exception cref="ApiParsingException">JSON cannot be deserialized</exception>
		/// <exception cref="ApiUnreachableException">API endpoint cannot be contacted</exception>
		public async Task<bool> LoadProviders(bool useGeodata = true)
		{
			Task loadLock = LoadProvidersLock;
			if (loadLock != null) try
			{
				loadLock.Wait();
			}
			catch (System.AggregateException) { }

			if (Online)
			{
				return true;
			}

			CancellationTokenSource cancel = null;
			if (Providers == null) try
			{
				cancel = new CancellationTokenSource();
				cancel.Token.ThrowIfCancellationRequested();
				LoadProvidersLock = Task.Delay(5000, cancel.Token);

				// downloads json file as string
				string apiJson = await DownloadUrlAsString(ProviderApiUrl, "application/json").ConfigureAwait(false);

				// gets api instance from json
				DiscoveryApi discovery = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson);
				Providers = discovery.Instances;

				if (ClosestProviders == null)
				{
					// turns out just running this once, even without saving it and caching makes subsequent calls much faster
					ClosestProviders = useGeodata ? await GetClosestProviders().ConfigureAwait(false) : Providers;
				}
			}
			catch (JsonSerializationException e)
			{
				throw new ApiParsingException("JSON does not fit object", e);
			}
			catch (JsonReaderException e)
			{
				throw new ApiParsingException("JSON contains syntax error", e);
			}
			catch (HttpRequestException e)
			{
				throw new ApiUnreachableException("Access to discovery API failed " + ProviderApiUrl, e);
			}
			catch (ApiParsingException)
			{
				throw;
			}
			finally
			{
				cancel.Cancel();
				cancel.Dispose();
				LoadProvidersLock = null;
			}
			return Online;
		}

		/// <summary>
		/// Will return the current coordinates of the users.
		/// It may download them if not cached
		/// </summary>
		private async Task<GeoCoordinate> GetCoordinates()
		{
			if (!GeoWatcher.Position.Location.IsUnknown)
			{
				return GeoWatcher.Position.Location;
			}

			try
			{
				return (await GetCurrentLocationFromGeoApi()).Geo.GeoCoordinate;
			}
			catch (ApiException)
			{
				return null;
			}
		}

		/// <exception cref="ApiUnreachableException">GeoApi download error</exception>
		/// <exception cref="ApiParsingException">GeoApi JSON error</exception>
		private async static Task<IdpLocation> GetCurrentLocationFromGeoApi()
		{
			try
			{
				string apiJson = await DownloadUrlAsString(GeoApiUrl, "application/json");
				return JsonConvert.DeserializeObject<IdpLocation>(apiJson);
			}
			catch (HttpRequestException e)
			{
				throw new ApiUnreachableException("GeoApi download error", e);
			}
			catch (JsonException e)
			{
				throw new ApiParsingException("GeoApi JSON error", e);
			}
		}

		/// <summary>
		/// Gets all profiles associated with a identity provider ID.
		/// </summary>
		/// <returns>identity provider profile object containing all profiles for given provider</returns>
		public List<IdentityProviderProfile> GetIdentityProviderProfiles(string idProviderId)
			=> idProviderId == null
				? Enumerable.Empty<IdentityProviderProfile>().ToList()
				: Providers.Where(p => p.Id == idProviderId).First().Profiles
				;

		private async Task<List<IdentityProvider>> GetClosestProviders()
		{
			if (Providers == null) return null;

			// find country code
			string closestCountryCode = null;
			try
			{
				// find country code from api
				closestCountryCode = (await GetCurrentLocationFromGeoApi()).Country;
			}
			catch (ApiException) { }

			if (null == closestCountryCode) {
				// gets country code as set in Settings
				// https://stackoverflow.com/questions/8879259/get-current-location-as-specified-in-region-and-language-in-c-sharp
				var regKeyGeoId = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International\Geo");
				var geoID = (string)regKeyGeoId.GetValue("Nation");
				var allRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.ToString()));
				var regionInfo = allRegions.FirstOrDefault(r => r.GeoId == Int32.Parse(geoID, CultureInfo.InvariantCulture));

				closestCountryCode = regionInfo.TwoLetterISORegionName;
			}

			try
			{
				var userCoords = await GetCoordinates();

				// sort and return n closest
				return Providers
					//.Where(p => p.Country == closestCountryCode)
					.OrderBy(p => userCoords.GetDistanceTo(p.GetClosestGeoCoordinate(userCoords)))
					.ToList();
			}
			catch (ApiUnreachableException)
			{
				return Providers
					//.Where(p => p.Country == closestCountryCode)
					.OrderByDescending(p => p.Country == closestCountryCode)
					.ToList();
			}
		}

		/// <summary>
		/// Gets download link for EAP config from json and downloads it.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ApiUnreachableException">Anything that went wrong attempting HTTP request, including DNS</exception>
		/// <exception cref="ApiParsingException">eap-config cannot be parsed as XML</exception>
		public async Task<EapConfig> DownloadEapConfig(string profileId)
		{
			IdentityProviderProfile profile = GetProfileFromId(profileId);
			if (String.IsNullOrEmpty(profile?.eapconfig_endpoint))
			{
				throw new EduroamAppUserException("Requested profile not listed in discovery");
			}

			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			// gets url to EAP config file download from GenerateEapConfig object
			Uri endpoint = new Uri(profile?.eapconfig_endpoint);

			// downloads and returns eap config file as string
			try
			{
				string eapXml = await DownloadUrlAsString(endpoint, "application/eap-config");
				return EapConfig.FromXmlData(profileId: profileId, eapXml);
			}
			catch (HttpRequestException e)
			{
				throw new ApiUnreachableException("Access to eap-config endpoint failed " + endpoint, e);
			}
			catch (XmlException e)
			{
				throw new ApiParsingException(e.Message + ": " + endpoint, e);
			}
		}

		/// <summary>
		/// Find IdentityProviderProfile by profileId string
		/// </summary>
		/// <exception cref="NullReferenceException">If LoadProviders() was not called or threw an exception</exception>
		/// <param name="profileId"></param>
		/// <returns>The IdentityProviderProfile with the given profileId</returns>
		public IdentityProviderProfile GetProfileFromId(string profileId)
		{
			foreach (IdentityProvider provider in Providers)
			{
				foreach (IdentityProviderProfile profile in provider.Profiles)
				{
					if (profile.Id == profileId)
					{
						return profile;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Gets a payload as string from url.
		/// </summary>
		/// <param name="url">Url that must be retrieved</param>
		/// <param name="accept">Content-Type to be expected, null for no check</param>
		/// <returns>HTTP body</returns>
		/// <exception cref="HttpRequestException">Anything that went wrong attempting HTTP request, including DNS</exception>
		/// <exception cref="ApiParsingException">Content-Type did not match accept</exception>
		private async static Task<string> DownloadUrlAsString(Uri url, string accept = null)
		{
			HttpResponseMessage response;
			try
			{
				response = await client.GetAsync(url);
			}
			catch (TaskCanceledException e)
			{
				// According to the documentation from HttpClient,
				// this exception will not be thrown, but instead a HttpRequestException
				// will be thrown.  This is not the case, so this catch and throw
				// is to make sure the API matches again
				throw new HttpRequestException("The request to " + url + " was interrupted", e);
			}

			// Check that we got the correct ContentType
			// Fun fact: did you know that headers are split into two categories?
			// There's both response.Headers and response.Content.Headers.
			// Don't try looking for headers at the wrong place, you'll get a System.InvalidOperationException!
			if (null != accept && accept != response.Content.Headers.ContentType.MediaType)
			{
				throw new ApiParsingException("Expected '" + accept + "' but got '" + response.Content.Headers.ContentType.MediaType);
			}

			HttpContent responseContent = response.Content;

			using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync().ConfigureAwait(false)))
			{
				return await reader.ReadToEndAsync().ConfigureAwait(false);
			}
		}

#pragma warning disable CA2227 // Collection properties should be read only
		private class DiscoveryApi
		{
			public int Version { get; set; }
			public int Seq { get; set; }
			public List<IdentityProvider> Instances { get; set; }
		}
#pragma warning restore CA2227 // Collection properties should be read only


		// Protected implementation of Dispose pattern.
		private bool _disposed = false;
		public void Dispose() => Dispose(true);
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing) GeoWatcher?.Dispose();
			_disposed = true;
		}

	}

}
