using System;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace EduroamConfigure
{
    /// <summary>
    /// Performs necessary steps in order to let the user authenticate through Feide.
    /// </summary>
    class OAuth
    {
        /// <summary>
        /// Gets authorization endpoints and calls method for browser authentication to get an EAP-config file.
        /// </summary>
        /// <param name="baseUrl">URL containing an encoded json string with endpoints.</param>
        /// <returns>EAP-config file as string.</returns>
        public static string BrowserAuthenticate(string baseUrl)
        {
            // downloads html file from url as string
            string letsWifiHtml;
            try
            {
                letsWifiHtml = GetStringFromUrl(baseUrl);
            }
            catch (WebException ex)
            {
                //MessageBox.Show("Couldn't fetch content from webpage. \nException: " + ex.Message, 
                //                "eduroam - Web exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            
            // gets the base64 encoded json containing the authorization endpoint from html
            string jsonString = GetBase64AndDecode(letsWifiHtml);
            // if no json found in html, stop execution
            if (string.IsNullOrEmpty(jsonString))
            {
               // MessageBox.Show("HTML doesn't contain authorization endpoint json.");
                return "";
            }

            // authorization endpoint
            string authEndpoint;
            // token endpoint
            string tokenEndpoint;
            // eap config generator endpoint
            string generatorEndpoint;

            // gets JObject containing OAuth endpoints from json string
            try
            {
                JObject endpointJson = JObject.Parse(jsonString);
                // gets individual endpoints
                authEndpoint = endpointJson["authorization_endpoint"].ToString();
                tokenEndpoint = endpointJson["token_endpoint"].ToString();
                generatorEndpoint = endpointJson["generator_endpoint"].ToString();
            }
            catch (JsonReaderException ex)
            {
               // MessageBox.Show("Couldn't read endpoints from JSON file.\n" +
               //                 "Exception: " + ex.Message, "JSON endpoints", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            
            // sets authorization uri parameters
            const string responseType = "code";
            const string codeChallengeMethod = "S256";
            const string scope = "eap-metadata";
            string codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase()); // generate random byte array, convert to base64url
            string codeChallenge = Base64UrlEncode(HashWithSHA256(codeVerifier)); // hash code verifier with SHA256, convert to base64url
            const string redirectUri = "http://localhost:8080/";
            const string clientId = "f817fbcc-e8f4-459e-af75-0822d86ff47a";
            string state = Base64UrlEncode(Guid.NewGuid().ToByteArray()); // random alphanumeric string
            const string grantType = "authorization_code";

            // concatenates parameters into authorization endpoint URI
            string authUri = string.Concat(authEndpoint, "?", ConstructQueryString(new NameValueCollection() {
                { "response_type", responseType },
                { "code_challenge_method", codeChallengeMethod },
                { "scope", scope },
                { "code_challenge", codeChallenge },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "state", state }
            }));

            // opens web browser for user authentication through feide
            string responseUrl; //= WebServer.NonblockingListener(redirectUri, authUri, parentLocation);
            using (var waitForm = new frmWaitDialog(redirectUri, authUri))
            {
                DialogResult result = waitForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    responseUrl = waitForm.responseUrl;
                }
                else
                {
                    return "";
                }
            }

            // checks if returned url is not empty
            if (string.IsNullOrEmpty(responseUrl))
            {
                MessageBox.Show("HTTP request returned nothing.");
                return "";
            }

            // checks if user chose to reject authorization
            if (responseUrl.Contains("access_denied"))
            {
                MessageBox.Show("Authorization rejected. Please try again.");
                return "";
            }

            // convert response url string to URI object
            var responseUri = new Uri(responseUrl);

            // gets state from response url and compares it to original state
            string newState = HttpUtility.ParseQueryString(responseUri.Query).Get("state");
            // checks if state has remained, if not cancel operation
            if (newState != state)
            {
                MessageBox.Show("State from request and response do not match. Aborting operation.\n", 
                    "Eduroam - State exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            // gets code from response url
            string code = HttpUtility.ParseQueryString(responseUri.Query).Get("code");
            // checks if code is not empty
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Response string doesn't contain code. Aborting operation.\n",
                    "Eduroam - Code exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            
                
            // concatenates parameters into token endpoint URI
            NameValueCollection tokenPostData = new NameValueCollection() {
                { "grant_type", grantType },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "code_verifier", codeVerifier }
            };


            string tokenJsonString;
            // downloads json file from url as string
            try
            {
                tokenJsonString = PostFormToUrl(tokenEndpoint, tokenPostData);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Couldn't fetch token json. \nException: " + ex.Message,
                    "Eduroam - Web exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            // token for authorizing Oauth request
            string token;
            // token type
            string tokenType;

            // gets JObject containing token information from json string
            try
            {
                JObject tokenJson = JObject.Parse(tokenJsonString);
                // gets token and type strings
                token = tokenJson["access_token"].ToString();
                tokenType = tokenJson["token_type"].ToString();
            }
            catch (JsonReaderException ex)
            {
                MessageBox.Show("Couldn't read token from JSON file.\n" +
                                "Exception: " + ex.Message, "JSON token", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            // gets and returns EAP config file as a string
            try
            {
                using (var client = new WebClient())
                {
                    // adds new header containing authorization token
                    client.Headers.Add("Authorization", tokenType + " " + token);
                    // downloads file
                    string eapConfigString = client.DownloadString(generatorEndpoint + "?format=eap-metadata");
                    return eapConfigString;
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("Couldn't fetch EAP config file. \nException: " + ex.Message,
                    "Eduroam - Web exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }
        
        /// <summary>
        /// Downloads web page content as a string.
        /// </summary>
        /// <param name="url">Url to download from.</param>
        /// <returns>Web page content.</returns>
        public static string GetStringFromUrl(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// Upload form and return data as a string.
        /// </summary>
        /// <param name="url">Url to upload to.</param>
        /// <param name="data">Data to post.</param>
        /// <returns>Web page content.</returns>
        public static string PostFormToUrl(string url, NameValueCollection data)
        {
            using (var client = new WebClient())
            {
                return Encoding.UTF8.GetString(client.UploadValues(url, "POST", data));
            }
        }

        /// <summary>
        /// Extracts base64 string from html and decodes it to get json with authorization endpoints.
        /// </summary>
        /// <param name="html">HTML containing authorization endpoints.</param>
        /// <returns>Json with authorization endpoints.</returns>
        private static string GetBase64AndDecode(string html)
        {
            const string beginString = "-----BEGIN LETSWIFI BLOCK-----";
            const string endString = "-----END LETSWIFI BLOCK-----";
            int indexOfBegin = html.IndexOf(beginString, StringComparison.Ordinal) + beginString.Length;
            int indexOfEnd = html.LastIndexOf(endString, StringComparison.Ordinal);

            if (indexOfBegin > 0 && indexOfEnd > 0)
            {
                string substring = html.Substring(indexOfBegin, indexOfEnd - indexOfBegin);
                byte[] data = Convert.FromBase64String(substring);
                string decodedString = Encoding.UTF8.GetString(data).Replace(@"\", "");
                return decodedString;
            }
            return "";
        }

        /// <summary>
        /// Generates a random code challenge base to use for the code challenge.
        /// </summary>
        /// <returns>Code challenge base.</returns>
        private static byte[] GenerateCodeChallengeBase()
        {
            var salt = new byte[32];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Converts a byte array to a base64url string.
        /// </summary>
        /// <param name="arg">Byte array.</param>
        /// <returns>Base64url string.</returns>
        private static string Base64UrlEncode(byte[] arg)
        {
            string s = Convert.ToBase64String(arg); // regular base64 encoder
            s = s.Split('=')[0]; // remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        /// <summary>
        /// Hashes a string using SHA256.
        /// </summary>
        /// <param name="dataString">String.</param>
        /// <returns>Hashed byte array.</returns>
        private static byte[] HashWithSHA256(string dataString)
        {
            // creates a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            }
        }

        /// <summary>
        /// Constructs a QueryString (string).
        /// Consider this method to be the opposite of "System.Web.HttpUtility.ParseQueryString"
        /// </summary>
        public static string ConstructQueryString(NameValueCollection parameters)
        {
            // https://leekelleher.com/2008/06/06/how-to-convert-namevaluecollection-to-a-query-string/
            List<string> items = new List<string>();

            foreach (string name in parameters)
                items.Add(string.Concat(System.Web.HttpUtility.UrlEncode(name), "=", System.Web.HttpUtility.UrlEncode(parameters[name])));

            return string.Join("&", items.ToArray());
        }
    }
}
