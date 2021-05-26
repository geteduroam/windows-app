using System;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Device.Location;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace EduroamConfigure
{
	public class IdentityProviderDownloader : IDisposable
	{
		// constants
		private static readonly Uri GeoApiUrl = new Uri("https://geo.eduroam.app/geoip");
#if DEBUG
		private static readonly Uri ProviderApiUrl = new Uri("https://discovery.eduroam.app/v1/discovery.json");
#else
		private static readonly Uri ProviderApiUrl = new Uri("https://discovery.eduroam.app/v1/discovery.json");
#endif

		// http objects
		private static readonly HttpClientHandler Handler = new HttpClientHandler
		{
			AutomaticDecompression = System.Net.DecompressionMethods.GZip | DecompressionMethods.Deflate,
			AllowAutoRedirect = true
		};
		private static readonly HttpClient Http = InitializeHttpClient();
		private static readonly GeoCoordinateWatcher GeoWatcher = new GeoCoordinateWatcher();

		// state
		private GeoCoordinate Coordinates; // Coordinates determined by OS or Web API
		private IdpLocation Location; // Location with country name determined by user setting or Web API
		private Task LoadProviderTask;
		private Task GeoWebApiTask;

		public IEnumerable<IdentityProvider> Providers { get; private set; }
		public IEnumerable<IdentityProvider> ClosestProviders
		{
			get => Coordinates != null && !Coordinates.IsUnknown
				? Providers.OrderBy(p => p.getDistanceTo(Coordinates))
				: Providers.OrderByDescending(p => p.Country == Location.Country)
				;
		}
		public bool Loaded { get => Providers.Any(); }
		public bool LoadedWithGeo { get => Loaded && Coordinates != null && !Coordinates.IsUnknown; }

		private static HttpClient InitializeHttpClient()
		{
			HttpClient client = new HttpClient(Handler, false);
#if DEBUG
			client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + "+DEBUG HttpClient (Windows NT 10.0; Win64; x64)");
#else
			client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + " HttpClient (Windows NT 10.0; Win64; x64)");
#endif
			client.Timeout = new TimeSpan(0, 0, 3);
			return client;
		}
		static IdentityProviderDownloader()
		{
			_ = Task.Run(() => GeoWatcher.Start(true));
		}

		/// <summary>
		/// The constructor for this class.
		/// Will download the list of all providers
		/// </summary>
		public IdentityProviderDownloader()
		{
			Providers = Enumerable.Empty<IdentityProvider>();

			// gets country code as set in Settings
			// https://stackoverflow.com/questions/8879259/get-current-location-as-specified-in-region-and-language-in-c-sharp
			var regKeyGeoId = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International\Geo");
			var geoID = (string)regKeyGeoId.GetValue("Nation");
			var allRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.ToString()));
			var regionInfo = allRegions.FirstOrDefault(r => r.GeoId == Int32.Parse(geoID, CultureInfo.InvariantCulture));

			Coordinates = GeoWatcher.Position.Location;
			Location = new IdpLocation
			{
				Country = regionInfo.TwoLetterISORegionName
			};
			Debug.Print("Geolocate OS API found country {0}, coordinates {1}", Location.Country, Coordinates);

			GeoWatcher.PositionChanged += (sender, e) =>
			{
				Coordinates = e.Position.Location;
				Debug.Print("Geolocate OS API found country {0}, coordinates {1}", Location.Country, Coordinates);
			};
		}

		/// <exception cref="ApiParsingException">JSON cannot be deserialized</exception>
		/// <exception cref="ApiUnreachableException">API endpoint cannot be contacted</exception>
		public Task LoadProviders(bool useGeodata)
		{
			if (LoadProviderTask == null || LoadProviderTask.IsCompleted && !Providers.Any())
			{
				LoadProviderTask = LoadProvidersInternal();
			}
			if (useGeodata && (GeoWebApiTask == null || GeoWebApiTask.IsCompleted && !LoadedWithGeo))
			{
				GeoWebApiTask = LoadGeoWebApi();
			}

			if (GeoWebApiTask != null && !GeoWebApiTask.IsCompleted)
			{
				return Task.Run(() =>
				{
					// Run the geolocation code async, but return after 700 milliseconds without aborting it
					// If geolocation is too slow, we don't want to keep the user waiting for that
					Task.WaitAll(new Task[] { LoadProviderTask, GeoWebApiTask }, 700);
					LoadProviderTask.Wait();
				});
			}
			return LoadProviderTask;
		}
		private Task LoadGeoWebApi()
		{
			var webTask = Task.Run(async () => {
				string apiJson = await DownloadUrlAsString(GeoApiUrl, new string[] { "application/json" }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<IdpLocation>(apiJson);
			});

			return Task.Run(() => {
				webTask.Wait();
				Location = webTask.Result ?? Location;
				Coordinates = Location.GeoCoordinate;

				Debug.Print("Geolocate Web API found country {0}, coordinates {1}", Location.Country, Coordinates);
			});
		}
		private async Task LoadProvidersInternal()
		{
			try
			{
				if (!Providers.Any())
				{
					// downloads json file as string
					string apiJson = await DownloadUrlAsString(ProviderApiUrl, new string[] { "application/json" }).ConfigureAwait(false);

					// gets api instance from json
					DiscoveryApi discovery = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson);
					Providers = discovery.Instances;
				}
			}
			catch (JsonSerializationException e)
			{
				Debug.WriteLine("THIS SHOULD NOT HAPPEN");
				Debug.Print(e.ToString());
				Debug.Assert(false);

				throw new ApiParsingException("JSON does not fit object", e);
			}
			catch (JsonReaderException e)
			{
				Debug.WriteLine("THIS SHOULD NOT HAPPEN");
				Debug.Print(e.ToString());
				Debug.Assert(false);

				throw new ApiParsingException("JSON contains syntax error", e);
			}
			catch (HttpRequestException e)
			{
				throw new ApiUnreachableException("Access to discovery API failed " + ProviderApiUrl, e);
			}
			catch (ApiParsingException)
			{
				// Logged upstream
				throw;
			}
		}

		/// <summary>
		/// Gets all profiles associated with a identity provider ID.
		/// </summary>
		/// <returns>identity provider profile object containing all profiles for given provider</returns>
		/// <param name="idProviderId">Find profiles belonging to provider with this ID</param>
		/// <exception cref="InvalidOperationException">There is no provider with id provided in <paramref name="idProviderId"/></exception>
		public List<IdentityProviderProfile> GetIdentityProviderProfiles(string idProviderId)
			=> Providers.Where(p => p.Id == idProviderId).First().Profiles;

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
			var eapConfig = await DownloadEapConfig(new Uri(profile.eapconfig_endpoint)).ConfigureAwait(true);
			eapConfig.ProfileId = profileId;
			return eapConfig;
		}
		public static async Task<EapConfig> DownloadEapConfig(Uri endpoint, string accessToken = null)
		{
			// downloads and returns eap config file as string
			try
			{
				string eapXml = await DownloadUrlAsString(
						url: endpoint,
						accept: new string[]{ "application/eap-config", "application/x-eap-config"},
						accessToken: accessToken
					);
				return EapConfig.FromXmlData(eapXml);
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
			if (!Loaded)
			{
				throw new EduroamAppUserException("not_online", "Cannot retrieve profile when offline");
			}

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
		private async static Task<string> DownloadUrlAsString(Uri url, string[] accept = null, string accessToken = null)
		{
			HttpResponseMessage response;
			try
			{
				if (accessToken == null)
				{
					response = await Http.GetAsync(url);
				}
				else
				{
					using var request = new HttpRequestMessage
					{
						Method = HttpMethod.Post,
						Content = new StringContent(""),
						RequestUri = url
					};
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					response = await Http.SendAsync(request).ConfigureAwait(true);
				}
			}
			catch (TaskCanceledException e)
			{
				// According to the documentation from HttpClient,
				// this exception will not be thrown, but instead a HttpRequestException
				// will be thrown.  This is not the case, so this catch and throw
				// is to make sure the API matches again
				throw new HttpRequestException("The request to " + url + " was interrupted", e);
			}

			return await parseResponse(response, accept);
		}
		/// <summary>
		/// Upload form and return data as a string.
		/// </summary>
		/// <param name="url">Url to upload to</param>
		/// <param name="data">Data to post</param>
		/// <param name="accept">Content-Type to be expected, null for no check</param>
		/// <returns>Response payload</returns>
		/// <exception cref="HttpRequestException">Anything that went wrong attempting HTTP request, including DNS</exception>
		/// <exception cref="ApiParsingException">Content-Type did not match accept</exception>
		public static Task<string> PostForm(Uri url, NameValueCollection data, string[] accept = null)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			var list = new List<KeyValuePair<string,string>>(data.Count);
			foreach(string key in data.AllKeys) {
				list.Add(new KeyValuePair<string,string>(key, data[key]));
			}
			return PostForm(url, list, accept);
		}

		/// <summary>
		/// Upload form and return data as a string.
		/// </summary>
		/// <param name="url">Url to upload to</param>
		/// <param name="data">Data to post</param>
		/// <param name="accept">Content-Type to be expected, null for no check</param>
		/// <returns>Response payload</returns>
		/// <exception cref="HttpRequestException">Anything that went wrong attempting HTTP request, including DNS</exception>
		/// <exception cref="ApiParsingException">Content-Type did not match accept</exception>
		public static async Task<string> PostForm(Uri url, IEnumerable<KeyValuePair<string, string>> data, string[] accept = null)
		{
			try
			{
				using var form = new FormUrlEncodedContent(data);
				var response = await Http.PostAsync(url, form);
				return await parseResponse(response, accept);
			}
			catch (TaskCanceledException e)
			{
				// According to the documentation from HttpClient,
				// this exception will not be thrown, but instead a HttpRequestException
				// will be thrown.  This is not the case, so this catch and throw
				// is to make sure the API matches again
				throw new HttpRequestException("The request to " + url + " was interrupted", e);
			}
		}

		private static async Task<string> parseResponse(HttpResponseMessage response, string[] accept)
		{
			if (response.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new HttpRequestException("HTTP " + response.StatusCode + ": " + response.ReasonPhrase + ": " + response.Content);
			}

			// Check that we got the correct ContentType
			// Fun fact: did you know that headers are split into two categories?
			// There's both response.Headers and response.Content.Headers.
			// Don't try looking for headers at the wrong place, you'll get a System.InvalidOperationException!
			if (null != accept && accept.Any() && !accept.Any((a) => a == response.Content.Headers.ContentType.MediaType))
			{
				throw new ApiParsingException("Expected one of '" + string.Join("', '", accept) + "' but got '" + response.Content.Headers.ContentType.MediaType);
			}

			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
		private bool _disposed;
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing) GeoWatcher?.Dispose();
			_disposed = true;
		}

	}

}
