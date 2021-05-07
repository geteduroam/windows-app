using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Device.Location;
using System.Globalization;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace EduroamConfigure
{
    public class IdentityProviderDownloader : IDisposable
    {
        // constants
        private const string GeoApiUrl = "https://geo.geteduroam.app/geoip";
#if DEBUG
        private const string ProviderApiUrl = "https://discovery.eduroam.app/v1/discovery.json";
#else
        private const string ProviderApiUrl = "https://discovery.eduroam.app/v1/discovery.json";
#endif

        // state
        public List<IdentityProvider> Providers { get; private set; }
        public List<IdentityProvider> ClosestProviders { get; private set; } // Providers presorted by geo distance
        private GeoCoordinateWatcher GeoWatcher { get; }
        public bool Online { get; set; }


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

        /// <exception cref="ApiUnreachableException">description</exception>
        /// <exception cref="ApiParsingException">description</exception>
        /// <exception cref="InternetConnectionException">When internet is offline</exception>
        public async void LoadProviders(bool useGeodata = true)
        {
            Online = false;
            Providers = await DownloadAllIdProviders();
            // turns out just running this once, even without saving it and caching makes subsequent calls much faster
            ClosestProviders = useGeodata ? await GetClosestProviders() : Providers;
            Online = true;
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
            return await DownloadCoordinates();
        }

        /// <summary>
        /// Fetches discovery data from geteduroam
        /// </summary>
        /// <returns>DiscoveryApi object with the ata fetched</returns>
        /// <exception cref="ApiUnreachableException">description</exception>
        /// <exception cref="ApiParsingException">description</exception>
        private async static Task<DiscoveryApi> DownloadDiscoveryApi()
        {
            try
            {
                // downloads json file as string
                string apiJson = await DownloadUrlAsString(ProviderApiUrl);
                // gets api instance from json
                DiscoveryApi apiInstance = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson);
                return apiInstance;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    throw new ApiUnreachableException($"Api for discovering Identity providers could not be reached. {ProviderApiUrl}", e);
                }
                else
                {
                    throw new InternetConnectionException($"Could not connect to the internet when trying to access API. {ProviderApiUrl}", e);
                }
            }
            catch (JsonReaderException ex)
            {
                throw new ApiParsingException($"Api for discovering Identity providers could not be parsed. {ProviderApiUrl}", ex);
            }
        }

        /// <exception cref="ApiUnreachableException">description</exception>
        /// <exception cref="ApiParsingException">description</exception>
        private async static Task<IdpLocation> GetCurrentLocationFromGeoApi()
        {
            try
            {
                string apiJson = await DownloadUrlAsString(GeoApiUrl);
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
        /// <exception cref="ApiUnreachableException">description</exception>
        /// <exception cref="ApiParsingException">description</exception>
        private async static Task<List<IdentityProvider>> DownloadAllIdProviders()
        {
            return (await DownloadDiscoveryApi()).Instances;
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
            // find country code
            string closestCountryCode;
            try
            {
                // find country code from api
                closestCountryCode = (await GetCurrentLocationFromGeoApi()).Country;
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

        private async static Task<GeoCoordinate> DownloadCoordinates()
        {
            return (await GetCurrentLocationFromGeoApi()).Geo.GeoCoordinate;
        }

        /// <summary>
        /// Gets download link for EAP config from json and downloads it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EduroamAppUserException">description</exception>
        public async Task<EapConfig> DownloadEapConfig(string profileId)
        {
            // adds profile ID to url containing json file, which in turn contains url to EAP config file download
            // gets url to EAP config file download from GenerateEapConfig object
            string endpoint = GetProfileFromId(profileId)?.eapconfig_endpoint;

            if (String.IsNullOrEmpty(endpoint)) {
                throw new EduroamAppUserException("Requested profile not listed in discovery");
            }

            // downloads and returns eap config file as string
            string eapXml;
            try
            {
                eapXml = await DownloadUrlAsString(endpoint);
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserException("WebException", WebExceptionToString(ex));
            }
            return EapConfig.FromXmlData(profileId: profileId, eapXml);
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
        /// Gets a json file as string from url.
        /// </summary>
        /// <param name="url">Url containing json file.</param>
        /// <returns>Json string.</returns>
        private async static Task<string> DownloadUrlAsString(string url)
        {
            // download json file from url as string
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.AllowAutoRedirect = true;
            var client = new HttpClient(handler);
#if DEBUG
            client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + "+DEBUG HttpClient (Windows NT 10.0; Win64; x64)");
#else
            client.DefaultRequestHeaders.Add("User-Agent", "geteduroam-win/" + LetsWifi.VersionNumber + " HttpClient (Windows NT 10.0; Win64; x64)");
#endif
            HttpResponseMessage response = await client.GetAsync(url);

            HttpContent responseContent = response.Content;

            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
			{
                return await reader.ReadToEndAsync();
            }
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
