using EduRoam.Connect.Exceptions;

using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EduRoam.Connect
{
    /// <summary>
    /// Performs necessary steps in order to let the user authenticate through Feide. The actual browser
    /// authentication has to be performed between GetAuthUri and GetEapConfigString.
    /// </summary>
    public class OAuth
    {
        // static config
        private const string ResponseType = "code";
        private const string CodeChallengeMethod = "S256";
        private const string Scope = "eap-metadata";
        public const string clientId = "app.geteduroam.win";
        // instance config
        private readonly Uri redirectUri; // uri to locally hosted servers
        private readonly Uri authEndpoint; // used to get authorization code through oauth
                                           // state created by CreateAuthUri
        private string codeVerifier;
        private string codeChallenge;
        private string state;

        /// <summary>
        /// Class used for OAuth process
        /// </summary>
        /// <param name="authEndpoint">Authorization Endpoint.</param>
        /// <param name="tokenEndpoint">Token Endpoint.</param>
        /// <param name="generatorEndpoint"> Generator endpoint for eap-config (resource endpoint).</param>
        public OAuth(Uri authEndpoint)
        {
            this.authEndpoint = authEndpoint;

            var rng = new Random();
            var randomPort = rng.Next(49152, 65535);
            this.redirectUri = new Uri($"http://[::1]:{randomPort}/");
        }

        /// <summary>
        /// Produces an authorization endpoint URI
        /// </summary>
        /// <returns>Authorization endpoint URI as string.</returns>
        public Uri CreateAuthUri()
        {
            // sets non-static authorization uri parameters
            this.state = Base64UrlEncode(Guid.NewGuid().ToByteArray()); // random alphanumeric string
            this.codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase()); // generate random byte array, convert to base64url
            this.codeChallenge = Base64UrlEncode(SHA256Hash(this.codeVerifier)); // hash code verifier with SHA256, convert to base64url

            // concatenates parameters into authorization endpoint URI
            var authUri = string.Concat(this.authEndpoint, "?", ConstructQueryString(new NameValueCollection() {
                { "response_type", ResponseType },
                { "code_challenge_method", CodeChallengeMethod },
                { "scope", Scope },
                { "code_challenge", this.codeChallenge },
                { "redirect_uri", this.redirectUri.ToString() },
                { "client_id", clientId },
                { "state", this.state }
            }));

            return new Uri(authUri);
        }

        public Uri GetRedirectUri()
            => this.redirectUri;


        /// <summary>
        /// Extracts the authorization code from the response url.
        /// </summary>
        /// <param name="responseUrl">URL response from authentication.</param>
        /// <returns>(string authorizationCode, string codeVerifier)</returns>
        public (string, string) ParseAndExtractAuthorizationCode(Uri responseUrl)
        {
            // check if url is valid
            if (!(responseUrl?.IsWellFormedOriginalString() ?? false)
                    || string.IsNullOrEmpty(responseUrl?.ToString()))
                throw new EduroamAppUserException("oauth empty reponse url",
                    userFacingMessage: "HTTP request returned nothing valid.");

            // Extract query parameters from response url
            var queryParams = HttpUtility.ParseQueryString(responseUrl.Query);

            // check if user chose to reject authorization
            if (queryParams.Get("error") == "access_denied")
                throw new EduroamAppUserException("oauth access denied",
                    userFacingMessage: "Authorization rejected. Please try again.");

            // get and check state from response url and compares it to original state
            var state = queryParams.Get("state");
            if (state != this.state)
                throw new EduroamAppUserException("oauth state mismatch",
                    userFacingMessage: "State from request and response do not match. Aborting operation.");

            // get and check code from response url
            var code = queryParams.Get("code");
            if (string.IsNullOrEmpty(code))
                throw new EduroamAppUserException("oauth code missing",
                    userFacingMessage: "Response string doesn't contain code. Aborting operation.");

            return (code, this.codeVerifier);
        }


        /// <summary>
        /// Generates a random code challenge base to use for the code challenge.
        /// </summary>
        /// <returns>Code challenge base.</returns>
        private static byte[] GenerateCodeChallengeBase()
        {
            using var random = RandomNumberGenerator.Create();

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
            var s = Convert.ToBase64String(arg); // regular base64 encoder
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
            using var sha256Hash = SHA256.Create();

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
