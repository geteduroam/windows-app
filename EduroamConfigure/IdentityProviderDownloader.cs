using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Device.Location;
using System.Globalization;

namespace EduroamConfigure
{
	public class IdentityProviderDownloader
	{
		// constants
		private const string GeoApiUrl = "https://geo.geteduroam.app/geoip"; // TODO: no access should result in still being able to search among discovery
#if DEBUG
		private const string ProviderApiUrl = "https://discovery.eduroam.app/v1/discovery.json";
#else
		private const string ProviderApiUrl = "https://discovery.geteduroam.app/v1/discovery.json";
#endif

		// state
		public List<IdentityProvider> Providers { get; }
		public List<IdentityProvider> ClosestProviders { get; }
		private GeoCoordinateWatcher GeoWatcher { get; }


		/// <summary>
		/// The constructor for this class.
		/// Will download the list of all providers
		/// </summary>
		/// <exception cref="ApiUnreachableException">description</exception>
		/// <exception cref="ApiParsingException">description</exception>
		public IdentityProviderDownloader()
		{
			GeoWatcher = new GeoCoordinateWatcher();
			GeoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
			Providers = DownloadAllIdProviders();
			// turns out just running this once, even without saving it and caching makes subsequent calls much faster
			ClosestProviders = GetClosestProviders();
		}

		/// <summary>
		/// Will return the current coordinates of the users.
		/// It may download them if not cached
		/// </summary>
		private GeoCoordinate GetCoordinates()
		{
			if (!GeoWatcher.Position.Location.IsUnknown)
			{
				return GeoWatcher.Position.Location;
			}
			return DownloadCoordinates();
		}

		/// <summary>
		/// Fetches discovery data from geteduroam
		/// </summary>
		/// <returns>DiscoveryApi object with the ata fetched</returns>
		/// <exception cref="ApiUnreachableException">description</exception>
		/// <exception cref="ApiParsingException">description</exception>
		private static DiscoveryApi DownloadDiscoveryApi()
		{
			try
			{
				// downloads json file as string
				string apiJson = DownloadUrlAsString(ProviderApiUrl);
				// gets api instance from json
				DiscoveryApi apiInstance = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson);
				return apiInstance;
			}
			catch (WebException ex)
			{
				throw new ApiUnreachableException($"Api for discovering Identity providers could not be reached. {ProviderApiUrl}", ex);
			}
			catch (JsonReaderException ex)
			{
				throw new ApiParsingException($"Api for discovering Identity providers could not be parsed. {ProviderApiUrl}", ex);
			}
		}

		/// <exception cref="ApiUnreachableException">description</exception>
		/// <exception cref="ApiParsingException">description</exception>
		private static IdpLocation GetCurrentLocationFromGeoApi()
		{
			try
			{
				string apiJson = DownloadUrlAsString(GeoApiUrl);
				return JsonConvert.DeserializeObject<IdpLocation>(apiJson);
			}
			catch (WebException ex)
			{
				throw new ApiUnreachableException("GeoApi download error", ex);
			}
			catch (JsonReaderException ex)
			{
				throw new ApiParsingException("GeoApi parsing error", ex);
			}
		}

		/// <summary>
		/// Downloads a list of all eduroam institutions
		/// </summary>
		/// <exception cref="EduroamAppUserError">description</exception>
		private static List<IdentityProvider> DownloadAllIdProviders()
		{
			return DownloadDiscoveryApi().Instances;
		}

		/// <summary>
		/// Gets all profiles associated with a identity provider ID.
		/// </summary>
		/// <returns>identity provider profile object containing all profiles for given provider</returns>
		public List<IdentityProviderProfile> GetIdentityProviderProfiles(int idProviderId)
		{
			return Providers.Where(p => p.cat_idp == idProviderId).First().Profiles;
		}

		/// <summary>
		/// Returns the n closest providers
		/// </summary>
		/// <param name="limit">number of providers to return</param>
		public List<IdentityProvider> GetClosestProviders(int limit)
		{
			 return ClosestProviders.Take(limit).ToList();
			// return GetClosestProviders().Take(limit).ToList();


		}

		private List<IdentityProvider> GetClosestProviders()
		{
			// find country code
			string closestCountryCode;
			try
			{
				// find country code from api
				closestCountryCode = GetCurrentLocationFromGeoApi().Country;
			}
			catch (ApiUnreachableException)
			{
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
				var userCoords = GetCoordinates();

				// sort and return n closest
				return Providers
					.Where(p => p.Country == closestCountryCode)
					.OrderBy(p => userCoords.GetDistanceTo(p.GetClosestGeoCoordinate(userCoords)))
					.ToList();
			}
			catch (ApiUnreachableException)
			{
				return Providers
					.Where(p => p.Country == closestCountryCode)
					.ToList();
			}
		}

		private static GeoCoordinate DownloadCoordinates()
		{
			return GetCurrentLocationFromGeoApi().Geo.GeoCoordinate;
		}

		/// <summary>
		/// Gets download link for EAP config from json and downloads it.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="EduroamAppUserError">description</exception>
		public string GetEapConfigString(string profileId)
		{
			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			// gets url to EAP config file download from GenerateEapConfig object
			string endpoint = GetProfileFromId(profileId).eapconfig_endpoint;

			// downloads and returns eap config file as string
			try
			{
				return DownloadUrlAsString(endpoint);
			}
			catch (WebException ex)
			{
				throw new EduroamAppUserError("WebException", WebExceptionToString(ex));
			}
		}

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
		/// Gets a json file as string from url.
		/// </summary>
		/// <param name="url">Url containing json file.</param>
		/// <returns>Json string.</returns>
		private static string DownloadUrlAsString(string url)
		{
			// download json file from url as string
			using var client = new WebClientWithTimeoutAndGzip();
			return client.DownloadString(url);
		}


		/// <summary>
		/// Produces error string for web exceptions that occur if user loses an existing connection.
		/// </summary>
		/// <param name="ex">WebException.</param>
		/// <returns>String containing error message meant for user to read.</returns>
		private static string WebExceptionToString(WebException ex)
		{
			return
				"Couldn't connect to the server.\n\n" +
				"Make sure that you are connected to the internet, then try again.\n" +
				"Exception: " + ex.Message;
		}



		/// <summary>
		/// WebClient with support for timeouts
		/// </summary>
		private class WebClientWithTimeoutAndGzip : WebClient
		{
			public int Timeout = 3000; // ms

			public WebClientWithTimeoutAndGzip() : base() {
				Encoding = Encoding.UTF8;
			}

			protected override WebRequest GetWebRequest(Uri uri)
			{
				HttpWebRequest w = base.GetWebRequest(uri) as HttpWebRequest;
				w.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
				w.Timeout = Timeout;
				return w;
			}
		}
	}

}
