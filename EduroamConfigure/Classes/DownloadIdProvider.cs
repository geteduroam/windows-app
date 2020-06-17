using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Device.Location;
using System.Globalization;
using System.Xml;
using Optional;

namespace EduroamConfigure
{
    class DownloadIdProvider
    {
        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        public Option<List<IdentityProvider>, string> GetAllInstitutions()
        {
            // url for json containing all identity providers/institutions
            const string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";
            try
            {
                // downloads json file as string
                string idProviderJson = GetStringFromUrl(allIdentityProvidersUrl);
                // gets list of identity providers from json file
                List<IdentityProvider> identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);
                // checks if json actually contains any identity providers, throws exception if not
                if (identityProviders.Count <= 0)
                {
                    throw new JsonReaderException("Institutions couldn't be read from JSON file.");
                }
                return Option.Some<List<IdentityProvider>, string>(identityProviders);
            }
            catch (WebException)
            {
                string error = "Couldn't connect to the server.\n\n"
                                + "Make sure that you are connected to the internet, then try again.";
                return Option.None<List<IdentityProvider>, string>(error);
            }
            catch (JsonReaderException)
            {
                string error = "Selecting an institution is not possible at the moment.\n\n" +
                                "Please try again later.";
                return Option.None<List<IdentityProvider>, string>(error);
            }

        }

        /// <summary>
        /// Compares institution coordinates with user's coordinates and gets the closest institution.
        /// </summary>
        /// <param name="instList">List of all institutions.</param>
        /// <param name="userCoord">User's coordinates.</param>
        /// <returns>Country of closest institution.</returns>
        public string GetClosestInstitution(List<IdentityProvider> instList)
        {
            //Start geowatcher to get coordinates
            GeoCoordinateWatcher geoWatcher = new GeoCoordinateWatcher();
            geoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            GeoCoordinate userCoord = geoWatcher.Position.Location;
            // institution's coordinates
            var instCoord = new GeoCoordinate();
            // closest institution
            var closestInst = new IdentityProvider();
            // shortest distance
            double shortestDistance = double.MaxValue;

            // loops through all institutions' coordinates and compares them with current shortest distance
            foreach (IdentityProvider inst in instList)
            {
                if (inst.Geo != null) // excludes if geo property not set
                {
                    // gets lat and long
                    instCoord.Latitude = inst.Geo.First().Lat;
                    instCoord.Longitude = inst.Geo.First().Lon;
                    // gets current distance
                    double currentDistance = userCoord.GetDistanceTo(instCoord);
                    // compares with shortest distance
                    if (currentDistance < shortestDistance)
                    {
                        // sets the current distance as the shortest dstance
                        shortestDistance = currentDistance;
                        closestInst = inst;
                    }
                }
            }
            // returns country of institution closest to user
            return closestInst.Country;
        }

        /// <summary>
        /// Gets a json file as string from url.
        /// </summary>
        /// <param name="url">Url containing json file.</param>
        /// <returns>Json string.</returns>
        public string GetStringFromUrl(string url)
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
        /// Gets a profile's attributes from json.
        /// </summary>
        /// <returns>Redirect link, if exists.</returns>
        public string GetProfileAttributes(string profileID)
        {
            // adds profile id to url
            string profileAttributeUrl = $"https://cat.eduroam.org/user/API.php?action=profileAttributes&id={profileID}&lang=en";

            // json file as string
            //deserialized json as Profile attributes objects
            IdProviderProfileAttributes profileAttributes;
            // downloads json from url
            string profileAttributeJson = GetStringFromUrl(profileAttributeUrl);
            // gets profile attributes from json
            profileAttributes = JsonConvert.DeserializeObject<IdProviderProfileAttributes>(profileAttributeJson);

            // checks profile attributes for a redirect link
            var redirect = "";
            foreach (var attribute in profileAttributes.Data.Devices)
            {
                if (attribute.Redirect != "0")
                {
                    redirect = attribute.Redirect;
                }
            }
            return redirect;
        }

        /// <summary>
        /// Gets download link for EAP config from json and downloads it.
        /// </summary>
        /// <returns></returns>
        public Option<string, string> GetEapConfigString(string profileID)
        {
            // adds profile ID to url containing json file, which in turn contains url to EAP config file download
            string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileID}";

            // contains json with eap config file download link
            GenerateEapConfig eapConfigInstance;
            try
            {
                // downloads json as string
                string generateEapJson = GetStringFromUrl(generateEapUrl);
                // converts json to GenerateEapConfig object
                eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);
            }
            catch (WebException ex)
            {
                return Option.None<string, string>(ex.Message);
            }
            catch (JsonReaderException ex)
            {
                return Option.None<string, string>(ex.Message);
            }

            // gets url to EAP config file download from GenerateEapConfig object
            string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

            // downloads and returns eap config file as string
            try
            {
                return Option.Some<string,string>(GetStringFromUrl(eapConfigUrl));
            }
            catch (WebException ex)
            {
                return Option.None<string, string>(ex.Message);
            }
        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        public Option<EapConfig, string> DownloadEapConfig(string profileID)
        {
            // checks if user has selected an institution and/or profile
            if (string.IsNullOrEmpty(profileID))
            {
                string error = "Please select an institution and/or a profile.";
                return Option.None<EapConfig, string>(error);
            }

            // checks for redirect link in profile attributes
            string redirect = GetProfileAttributes(profileID);
            // eap config file as string
            string eapString;

            // if no redirect link
            if (string.IsNullOrEmpty(redirect))
            {
                // gets eap config file directly
                eapString = GetEapConfigString(profileID).ValueOr("null");
            }
            // if Let's Wifi redirect
            else if (redirect.Contains("#letswifi"))
            {
                // get eap config file from browser authenticate
                eapString = OAuth.BrowserAuthenticate(redirect);

            }
            // if other redirect
            else
            {
                // makes redirect link accessible in parent form
                return Option.None<EapConfig, string>("null");
            }

            // if not empty, creates and returns EapConfig object from Eap string
            if (string.IsNullOrEmpty(eapString))
            {
                string error = "Could not generate EapString";
                return Option.None<EapConfig, string>(error);
            }

            try
            {
                // if not empty, creates and returns EapConfig object from Eap string
                return Option.Some<EapConfig, string>(ConnectToEduroam.GetEapConfig(eapString));
            }
            catch (XmlException ex)
            {
                string error = "Error fetching eap config";
                return Option.None<EapConfig, string>(error);
            }
        }



    }
}
