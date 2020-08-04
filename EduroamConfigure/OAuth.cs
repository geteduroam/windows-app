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
		/// Extracts the authorization code from the response url.
		/// </summary>
		/// <param name="responseUrl">URL response from authentication.</param>
		/// <returns>(string authorizationCode, string codeVerifier)</returns>
		public (string, string) ParseAndExtractAuthorizationCode(string responseUrl)
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

			return (code, codeVerifier);
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
