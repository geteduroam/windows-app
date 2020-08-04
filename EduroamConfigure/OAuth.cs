using System;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Performs necessary steps in order to let the user authenticate through Feide. The actual browser
    /// authentication has to be performed between GetAuthUri and GetEapConfigString.
    /// </summary>
    public class OAuth
    {
        // static config
        private const string responseType = "code";
        private const string codeChallengeMethod = "S256";
        private const string scope = "eap-metadata";
        private const string clientId = "app.geteduroam.win";
        // instance config
        private readonly Uri redirectUri;
        private readonly string profileId;
        private readonly string authEndpoint;
        private readonly string tokenEndpoint;
        private readonly string generatorEndpoint;
        // state created by CreateAuthUri
        private string codeVerifier;
        private string codeChallenge;
        private string state;

        public OAuth(IdentityProviderProfile profile) :
            this(
                profile?.authorization_endpoint,
                profile?.token_endpoint,
                profile?.eapconfig_endpoint,
                profile?.Id
            ) { }

        /// <summary>
        /// Class used for OAuth process
        /// </summary>
        /// <param name="authEndpoint">Authorization Endpoint.</param>
        /// <param name="tokenEndpoint">Token Endpoint.</param>
        /// <param name="generatorEndpoint"> Generator endpoint for eap-config (resource endpoint).</param>
        public OAuth(string authEndpoint, string tokenEndpoint, string generatorEndpoint, string profileId)
        {
            this.authEndpoint = authEndpoint;
            this.tokenEndpoint = tokenEndpoint;
            this.generatorEndpoint = generatorEndpoint;
            this.profileId = profileId;

            Random rng = new Random();
            int randomPort = rng.Next(49152, 65535);
            redirectUri = new Uri($"http://[::1]:{randomPort}/");
        }

        /// <summary>
        /// Produces an authorization endpoint URI
        /// </summary>
        /// <returns>Authorization endpoint URI as string.</returns>
        public Uri CreateAuthUri()
        {
            // sets non-static authorization uri parameters
            state = Base64UrlEncode(Guid.NewGuid().ToByteArray()); // random alphanumeric string
            codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase()); // generate random byte array, convert to base64url
            codeChallenge = Base64UrlEncode(SHA256Hash(codeVerifier)); // hash code verifier with SHA256, convert to base64url

            // concatenates parameters into authorization endpoint URI
            string authUri = string.Concat(authEndpoint, "?", ConstructQueryString(new NameValueCollection() {
                { "response_type", responseType },
                { "code_challenge_method", codeChallengeMethod },
                { "scope", scope },
                { "code_challenge", codeChallenge },
                { "redirect_uri", redirectUri.ToString() },
                { "client_id", clientId },
                { "state", state }
            }));

            return new Uri(authUri);
        }

        public Uri GetRedirectUri()
            => redirectUri;

        /// <summary>
        /// Uses URL containing response after authenticating using authUri from GetAuthUri to get an EAP-config file.
        /// </summary>
        /// <param name="reponseUrl">URL response from authentication.</param>
        /// <returns>EAP-config file as string.</returns>
        public EapConfig DownloadEapConfig(string responseUrl)
        {
            // check if url is not empty
            if (string.IsNullOrEmpty(responseUrl))
                throw new EduroamAppUserError("oauth empty reponse url",
                    userFacingMessage: "HTTP request returned nothing.");

            // check if user chose to reject authorization
            if (responseUrl.Contains("access_denied")) // TODO: this check sucks
                throw new EduroamAppUserError("oauth access denied",
                    userFacingMessage: "Authorization rejected. Please try again.");

            // Extract query parameters from response url
            var responseUrlQueryParams = HttpUtility.ParseQueryString(new Uri(responseUrl).Query);

            // get and check state from response url and compares it to original state
            string responseState = responseUrlQueryParams.Get("state");
            if (responseState != state)
                throw new EduroamAppUserError("oauth state mismatch",
                    userFacingMessage: "State from request and response do not match. Aborting operation.");

            // get and check code from response url
            string code = responseUrlQueryParams.Get("code");
            if (string.IsNullOrEmpty(code))
                throw new EduroamAppUserError("oauth code missing",
                    userFacingMessage: "Response string doesn't contain code. Aborting operation.");


            // concatenates parameters into token endpoint URI
            var tokenPostData = new NameValueCollection() {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri.ToString() },
                { "client_id", clientId },
                { "code_verifier", codeVerifier }
            };

            // downloads json file from url as string
            string tokenJsonString = PostFormGetResponse(tokenEndpoint, tokenPostData);

            // Parse json response to retrieve authorization tokens
            string accessToken;
            string accessTokenType;
            string refreshToken;
            int? refreshTokenExpiresIn;
            try
            {
                JObject tokenJson = JObject.Parse(tokenJsonString);

                accessToken = tokenJson["access_token"].ToString(); // token to retrieve EAP config
                accessTokenType = tokenJson["token_type"].ToString(); // Usually "Bearer", a http authorization scheme
                refreshToken = tokenJson["refresh_token"].ToString(); // token to refresh access token
                refreshTokenExpiresIn = tokenJson["expires_in"].ToObject<int?>();
            }
            catch (JsonReaderException ex)
            {
                throw new EduroamAppUserError("oauth unprocessable response",
                    userFacingMessage: "Couldn't read tokens from JSON file.\n" + "Exception: " + ex.Message);
            }

            // gets and returns EAP config file as a string
            string eapConfigXml;
            try
            {
                // Setup client with authorization token in header
                using var client = new WebClient();
                client.Headers.Add("Authorization", accessTokenType + " " + accessToken);

                // download file
                eapConfigXml = client.DownloadString(generatorEndpoint + "?format=eap-metadata"); // TODO: use a uri builder or something
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserError("oauth eapconfig get error",
                    userFacingMessage: "Couldn't fetch EAP config file. \nException: " + ex.Message);
            }
            return EapConfig.FromXmlData(uid: profileId, eapConfigXml);
        }

        /// <summary>
        /// Upload form and return data as a string.
        /// </summary>
        /// <param name="url">Url to upload to.</param>
        /// <param name="data">Data to post.</param>
        /// <returns>Web page content.</returns>
        public static string PostFormGetResponse(string url, NameValueCollection data)
        {
            try
            {
                using var client = new WebClient();
                return Encoding.UTF8.GetString(client.UploadValues(url, "POST", data));
            }
            catch (WebException ex)
            {
                throw new EduroamAppUserError("oauth post error",
                    userFacingMessage: "Couldn't fetch token json.\nException: " + ex.Message);
            }
        }

        /// <summary>
        /// Generates a random code challenge base to use for the code challenge.
        /// </summary>
        /// <returns>Code challenge base.</returns>
        private static byte[] GenerateCodeChallengeBase()
        {
            using var random = new RNGCryptoServiceProvider();

            var salt = new byte[32];
            random.GetNonZeroBytes(salt);
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
            s = s.Split('=')[0]; // remove trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        /// <summary>
        /// Hashes a string using SHA256.
        /// </summary>
        /// <param name="dataString">String.</param>
        /// <returns>Hashed byte array.</returns>
        private static byte[] SHA256Hash(string dataString)
        {
            // create a SHA256 context
            using SHA256 sha256Hash = SHA256.Create();

            // Compute hash and return
            return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
        }

        /// <summary>
        /// Constructs a QueryString (string).
        /// Consider this method to be the opposite of "System.Web.HttpUtility.ParseQueryString"
        /// </summary>
        private static string ConstructQueryString(NameValueCollection parameters)
        {
            _ = parameters ?? throw new ArgumentNullException(paramName: nameof(parameters));

            // TODO: this function should be removed and replaced with some uribuilder thing

            return string.Join("&", 
                parameters.AllKeys
                    .Select(key => (key, value: parameters[key]))
                    .Select(i => string.Concat(HttpUtility.UrlEncode(i.key), "=", HttpUtility.UrlEncode(i.value)))
            );
        }

    }
}
