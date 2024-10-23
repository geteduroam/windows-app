using App.Settings;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Localization;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Management;

using EduRoam.Connect.Converter;
using EduRoam.Connect.Identity.v2;

namespace EduRoam.Connect.Identity
{
    public class IdentityProviderDownloader : IDisposable
    {
        // constants
        private static readonly Uri ProviderApiUrl = new Configuration().ProviderApiUrl;

        // http objects
        private static readonly HttpClientHandler Handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true
        };
        private static readonly HttpClient Http = InitializeHttpClient();

        // state
        private readonly IdpLocation location; // Location with country name determined by user setting or Web API
        private Task? loadProviderTask;

        public IEnumerable<IdentityProvider> Providers { get; private set; }

        public IEnumerable<IdentityProvider> ClosestProviders
        {
            get => this.Providers.OrderByDescending(p => p.Country == this.location.Country);
        }

        public bool Loaded { get => this.Providers.Any(); }

        private static HttpClient InitializeHttpClient()
        {
            var r = string.Empty;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                var information = searcher.Get();
                if (information != null)
                {
                    foreach (ManagementObject obj in information)
                    {
                        r = obj["Caption"].ToString() + "; " + obj["OSArchitecture"].ToString();
                    }
                }
                r = r.Replace("NT 5.1.2600", "XP");
                r = r.Replace("NT 5.2.3790", "Server 2003");
            }

            var client = new HttpClient(Handler, false);
#if DEBUG
            client.DefaultRequestHeaders.Add("User-Agent", $"{Settings.ApplicationIdentifier}-win/{LetsWifi.Instance.VersionNumber} DEBUG HttpClient ({r})");
#else
            client.DefaultRequestHeaders.Add("User-Agent", $"{Settings.ApplicationIdentifier}-win/{LetsWifi.Instance.VersionNumber} HttpClient ({r})");
#endif
            // This client will not be used for subsequent requests,
            // so don't keep the connection open any longer than necessary
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = new TimeSpan(0, 0, 8);
            return client;
        }

        /// <summary>
        /// The constructor for this class.
        /// Will download the list of all providers
        /// </summary>
        public IdentityProviderDownloader()
        {
            this.Providers = Enumerable.Empty<IdentityProvider>();

            // gets country code as set in Settings
            // https://stackoverflow.com/questions/8879259/get-current-location-as-specified-in-region-and-language-in-c-sharp
            var regKeyGeoId = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International\Geo");
            var geoID = (string?)regKeyGeoId?.GetValue("Nation");
            var allRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.ToString()));
            var regionInfo = allRegions.FirstOrDefault(r => geoID != null && r.GeoId == int.Parse(geoID, CultureInfo.InvariantCulture));

            this.location = new IdpLocation
            {
                Country = regionInfo?.TwoLetterISORegionName ?? ""
            };
            Debug.Print("Found country {0}", this.location.Country);
        }

        /// <exception cref="ApiParsingException">JSON cannot be deserialized</exception>
        /// <exception cref="ApiUnreachableException">API endpoint cannot be contacted</exception>
        public async Task LoadProviders()
        {
            if (this.loadProviderTask == null || (this.loadProviderTask.IsCompleted && !this.Providers.Any()))
            {
                this.loadProviderTask = this.LoadProvidersInternal();
            }
            await this.loadProviderTask;
        }

        private async Task LoadProvidersInternal()
        {
            try
            {
                if (Cache.IdentityProviders != null)
                {
                    this.Providers = Cache.IdentityProviders;
                    return;
                }
                
                if (!this.Providers.Any())
                {
                    var isNewVersion = ProviderApiUrl.ToString().Contains("/v2/");
                    
                    // downloads json file as string
                    var apiJson = await DownloadUrlAsString(ProviderApiUrl, new string[] { "application/json" }).ConfigureAwait(false);

                    DiscoveryApi discoveryData;

                    if (isNewVersion)
                    {
                        var discovery = JsonConvert.DeserializeObject<LetsWifiDiscovery>(apiJson);
                        discoveryData = DiscoveryConverter.Covert(discovery ?? new LetsWifiDiscovery());
                    }
                    else
                    {
                        discoveryData = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson) ?? new DiscoveryApi();
                    }
                    
                    this.Providers = discoveryData?.Instances ?? new List<IdentityProvider>();
                    Cache.IdentityProviders = (List<IdentityProvider>?)this.Providers;
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
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
        }

        /// <summary>
        /// Gets all profiles associated with a identity provider ID.
        /// </summary>
        /// <returns>identity provider profile object containing all profiles for given provider</returns>
        /// <param name="idProviderId">Find profiles belonging to provider with this ID</param>
        /// <exception cref="InvalidOperationException">There is no provider with id provided in <paramref name="idProviderId"/></exception>
        public List<IdentityProviderProfile> GetIdentityProviderProfiles(string idProviderId)
            => this.Providers.Where(p => p.Id == idProviderId).First().Profiles;

        /// <summary>
        /// Gets download link for EAP config from json and downloads it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiUnreachableException">Anything that went wrong attempting HTTP request, including DNS</exception>
        /// <exception cref="ApiParsingException">eap-config cannot be parsed as XML</exception>
        public async Task<EapConfig> DownloadEapConfig(string profileId)
        {
            await this.LoadProviders();
            var profile = await this.GetProfileFromId(profileId);
            if (string.IsNullOrEmpty(profile?.EapConfigEndpoint))
            {
                throw new EduroamAppUserException("Requested profile not listed in discovery");
            }

            // adds profile ID to url containing json file, which in turn contains url to EAP config file download
            // gets url to EAP config file download from GenerateEapConfig object
            var eapConfig = await DownloadEapConfig(new Uri(profile.EapConfigEndpoint)).ConfigureAwait(true);
            eapConfig.ProfileId = profileId;
            return eapConfig;
        }

        public static async Task<EapConfig> DownloadEapConfig(Uri endpoint, string? accessToken = null)
        {
            // downloads and returns eap config file as string
            try
            {
                var eapXml = await DownloadUrlAsString(
                        url: endpoint,
                        accept: new string[] { "application/eap-config", "application/x-eap-config" },
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
        /// <param name="profileId"></param>
        /// <returns>The IdentityProviderProfile with the given profileId</returns>
        /// 
        /// <exception cref="EduroamAppUserException">If LoadProviders() was not called or threw an exception</exception>
        public async Task<IdentityProviderProfile?> GetProfileFromId(string profileId)
        {
            if (!this.Loaded)
            {
                throw new EduroamAppUserException("not_online", Resources.ErrorCannotRetrieveProfileWhenOffline);
            }

            foreach (var provider in this.Providers)
            {
                foreach (var profile in provider.Profiles)
                {
                    if (profile.Id == profileId)
                    {
                        if (!string.IsNullOrEmpty(profile.LetsWifiEndpoint))
                        {
                            var letsWifiProfile = await DownloadLetsWifiProfile(profile);
                            profile.EapConfigEndpoint = letsWifiProfile?.EapConfigEndpoint;
                            profile.TokenEndpoint = letsWifiProfile?.TokenEndpoint;
                            profile.AuthorizationEndpoint = letsWifiProfile?.AuthorizationEndpoint;
                        }
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
        /// 
        /// <exception cref="HttpRequestException">Anything that went wrong attempting HTTP request, including DNS</exception>
        /// <exception cref="ApiParsingException">Content-Type did not match accept</exception>
        private async static Task<string> DownloadUrlAsString(Uri url, string[]? accept = null, string? accessToken = null)
        {
            HttpResponseMessage response;
            try
            {
                using (var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = url
                })
                {
                    if (accessToken != null)
                    {
                        request.Method = HttpMethod.Post;
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }

                    foreach (var acceptValue in accept ?? new string[] { })
                    {
                        request.Headers.Add("Accept", acceptValue);
                    }

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
        public static Task<string> PostForm(Uri url, NameValueCollection data, string[]? accept = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var list = new List<KeyValuePair<string, string?>>(data.Count);
            foreach (var key in data.AllKeys.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                Debug.Assert(key != null, "data contains meta data items for retrieving a token");
                list.Add(new KeyValuePair<string, string?>(key, data[key]));
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
        public static async Task<string> PostForm(Uri url, IEnumerable<KeyValuePair<string, string?>> data, string[]? accept = null)
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

        private async Task<LetsWifiProfile.ProfileRoot> DownloadLetsWifiProfile(IdentityProviderProfile profile)
        {
            try
            {
                if (!string.IsNullOrEmpty(profile.LetsWifiEndpoint))
                {
                    var letsWifiProfileJson = await DownloadUrlAsString(
                        url: new Uri(profile.LetsWifiEndpoint), 
                        accept: ["application/json"], 
                        accessToken: null
                    );
                    var letsWifiProfile = JsonConvert.DeserializeObject<LetsWifiProfile>(letsWifiProfileJson);

                    return letsWifiProfile.Root;
                } else
                {
                    throw new Exception("No LetsWifi endpoint found in profile");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new EduroamAppUserException(e.Message, "Error occurred while retrieving LetsWifi profile");
            }

            return new();
        }

        private static async Task<string> parseResponse(HttpResponseMessage response, string[]? accept)
        {
            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("HTTP " + response.StatusCode + ": " + response.ReasonPhrase + ": " + response.Content);
            }

            // Check that we got the correct ContentType
            // Fun fact: did you know that headers are split into two categories?
            // There's both response.Headers and response.Content.Headers.
            // Don't try looking for headers at the wrong place, you'll get a System.InvalidOperationException!
            if (null != accept && accept.Any() && !accept.Any((a) => a == response.Content.Headers.ContentType?.MediaType))
            {
                throw new ApiParsingException("Expected one of '" + string.Join("', '", accept) + "' but got '" + response.Content.Headers.ContentType?.MediaType);
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        // Protected implementation of Dispose pattern.
        private bool disposed;
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;

            this.disposed = true;
        }

    }

}
