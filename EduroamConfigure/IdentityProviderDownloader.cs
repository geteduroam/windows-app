using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Device.Location;

namespace EduroamConfigure
{
    public class IdentityProviderDownloader
    {
        public List<IdentityProvider> Providers;
        private GeoCoordinateWatcher GeoWatcher;

        public IdentityProviderDownloader()
        {
            GeoWatcher = new GeoCoordinateWatcher();
            GeoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            this.Providers = GetAllIdProviders();
        }


        private GeoCoordinate GetCoordinates()
        {
            if (!GeoWatcher.Position.Location.IsUnknown)
            {
                return GeoWatcher.Position.Location;
            } 
            return DownloadCoordinates();
        }


        /// <summary>
        /// Fetches data from  https://discovery.geteduroam.no/v1/discovery.json and turns it into a DiscoveryApi object
        /// </summary>
        /// <returns>DiscoveryApi object representing the Api</returns>
        /// <exception cref="EduroamAppUserError">description</exception>

        private static DiscoveryApi GetDiscoveryApi()
        {
            string apiUrl = "https://discovery.geteduroam.no/v1/discovery.json";
            try
            {
                // downloads json file as string
                string apiJson = GetStringFromUrl(apiUrl);
                // gets api instance from json
                DiscoveryApi apiInstance = JsonConvert.DeserializeObject<DiscoveryApi>(apiJson);
                return apiInstance;
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserError("", GetWebExceptionString(ex));
            }
            catch (JsonReaderException ex)
            {
                throw new EduroamAppUserError("", GetJsonExceptionString(ex));
            }
        }

        private static Location GetLocationApi()
        {
            string apiUrl = "https://geo.geteduroam.app/geoip";
            try
            {
                string apiJson = GetStringFromUrl(apiUrl);
                Location apiInstance = JsonConvert.DeserializeObject<Location>(apiJson);
                return apiInstance;
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserError("", GetWebExceptionString(ex));
            }
            catch (JsonReaderException ex)
            {
                throw new EduroamAppUserError("", GetJsonExceptionString(ex));
            }
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        /// <exception cref="EduroamAppUserError">description</exception>
        private static List<IdentityProvider> GetAllIdProviders()
        {
            return GetDiscoveryApi().Instances;
        }

        /// <summary>
        /// Gets all profiles associated with a identity provider ID.
        /// </summary>
        /// <returns>identity provider profile object containing all profiles for given provider</returns>
        /// <exception cref="EduroamAppUserError">description</exception>
        public List<IdentityProviderProfile> GetIdentityProviderProfiles(int idProviderId)
        {
            return Providers.Where(p => p.cat_idp == idProviderId).First().Profiles;
        }


        public List<IdentityProvider> GetClosestProviders(int n)
        {
            // find all providers in current country
            string closestCountryCode = GetLocationApi().Country;
            List<IdentityProvider> localProviders = Providers.Where(p => p.Country == closestCountryCode).ToList();
            var userCoords = GetCoordinates();
            // sort provider list by distance to user location
            localProviders = localProviders.OrderBy(
                p => userCoords.GetDistanceTo(p.GetClosestGeoCoordinate(userCoords))
            ).ToList();

            // return n closest
            return localProviders.Take(n).ToList();
           
        }

        private static GeoCoordinate DownloadCoordinates()
        {
            Location userLocation = GetLocationApi();
            Geo userGeo = userLocation.Geo;
            return userGeo.toGeoCoordinate();
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
                return GetStringFromUrl(endpoint);
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserError("WebException", GetWebExceptionString(ex));
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
        private static string GetStringFromUrl(string url)
        {
            // downloads json file from url as string
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                string jsonString = client.DownloadString(url);
                return jsonString;
            }
        }


        /// <summary>
        /// Produces error string for web exceptions that occur if user loses an existing connection.
        /// </summary>
        /// <param name="ex">WebException.</param>
        /// <returns>String containing error message meant for user to read.</returns>
        private static string GetWebExceptionString(WebException ex)
        {
            string error =   "Couldn't connect to the server.\n\n"
                    + "Make sure that you are connected to the internet, then try again.\n" +
                    "Exception: " + ex.Message;
            return error;
        }


        /// <summary>
        /// Produces error string for exceptions related to deserializing JSON files and corrupted XML files.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>String containing error message meant for user to read.</returns>
        private static string GetJsonExceptionString(Exception ex)
        {
            string error = "The selected institution or profile is not supported. " +
                            "Please select a different institution or profile.\n"
                            + "Exception: " + ex.Message;
            return error;
        }

    }
       
}
