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
	/// Performs necessary steps in order to let the user authenticate through Feide. The actual browser
	/// authentication has to be performed between GetAuthUri and GetEapConfigString.
	/// </summary>
	public class OAuth
	{
		private const string responseType = "code";
		private const string codeChallengeMethod = "S256";
		private const string scope = "eap-metadata";
		private string codeVerifier;
		private string codeChallenge;
		private const string clientId = "f817fbcc-e8f4-459e-af75-0822d86ff47a";
		private const string grantType = "authorization_code";
		private readonly string redirectUri;
		private readonly string authEndpoint;
		private readonly string tokenEndpoint;
		private readonly string generatorEndpoint;
		private string state;

		/// <summary>
		/// Class used for OAuth process
		/// </summary>
		/// <param name="authEndpoint">Authorization Endpoint.</param>
		/// <param name="tokenEndpoint">Token Endpoint.</param>
		/// <param name="generatorEndpoint"> Generator endpoint for eap-config (resource endpoint).</param>
		public OAuth(string authEndpoint, string tokenEndpoint, string generatorEndpoint)
		{
			this.authEndpoint = authEndpoint;
			this.tokenEndpoint = tokenEndpoint;
			this.generatorEndpoint = generatorEndpoint;
			Random rng = new Random();
			int randomPort = rng.Next(49152, 65535);
			redirectUri = $"http://[::1]:{randomPort}/";
		}

		/// <summary>
		/// Produces an authorization endpoint URI
		/// </summary>
		/// <returns>Authorization endpoint URI as string.</returns>
		public string GetAuthUri()
		{
			// sets non-static authorization uri parameters
			state = Base64UrlEncode(Guid.NewGuid().ToByteArray()); // random alphanumeric string
			codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase()); // generate random byte array, convert to base64url
			codeChallenge = Base64UrlEncode(HashWithSHA256(codeVerifier)); // hash code verifier with SHA256, convert to base64url


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

			return authUri;
		}


		/// <summary>
		/// Uses URL containing response after authenticating using authUri from GetAuthUri to get an EAP-config file.
		/// </summary>
		/// <param name="reponseUrl">URL response from authentication.</param>
		/// <returns>EAP-config file as string.</returns>
		public string GetEapConfigString(string responseUrl)
		{
			// checks if url is not empty
			if (string.IsNullOrEmpty(responseUrl))
			{
				string error = "HTTP request returned nothing.";
				throw new EduroamAppUserError("oath empty reponse url", error);
			}

			// checks if user chose to reject authorization
			if (responseUrl.Contains("access_denied"))
			{
				string error = "Authorization rejected. Please try again.";
				throw new EduroamAppUserError("oath access denied", error);
			}

			// Extract query parameters from response url
			var responseUrlQueryParams = HttpUtility.ParseQueryString(new Uri(responseUrl).Query);

			// gets state from response url and compares it to original state
			string newState = responseUrlQueryParams.Get("state");
			// checks if state has remained, if not cancel operation
			if (newState != state)
			{
				string error = "State from request and response do not match. Aborting operation.";
				throw new EduroamAppUserError("oath state mismatch", error);
			}

			// gets code from response url
			string code = responseUrlQueryParams.Get("code");
			// checks if code is not empty
			if (string.IsNullOrEmpty(code))
			{
				string error = "Response string doesn't contain code. Aborting operation.";
				throw new EduroamAppUserError("oath code missing", error);
			}



			// concatenates parameters into token endpoint URI
			var tokenPostData = new NameValueCollection() {
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
				string error = "Couldn't fetch token json. \nException: " + ex.Message;
				throw new EduroamAppUserError("oath post error", error);
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
				string error = "Couldn't read token from JSON file.\n" + "Exception: " + ex.Message;
				throw new EduroamAppUserError("oath unprocessable response", error);
			}

			// gets and returns EAP config file as a string
			try
			{
				// Setup client with authorization token in header
				using var client = new WebClient();
				client.Headers.Add("Authorization", tokenType + " " + token);

				// download file
				string eapConfigString = client.DownloadString(generatorEndpoint + "?format=eap-metadata");
				return eapConfigString;
			}
			catch (WebException ex)
			{
				string error = "Couldn't fetch EAP config file. \nException: " + ex.Message;
				throw new EduroamAppUserError("oath eapconfig get error", error);
			}
		}

		/// <summary>
		/// Downloads web page content as a string.
		/// </summary>
		/// <param name="url">Url to download from.</param>
		/// <returns>Web page content.</returns>
		public static string GetStringFromUrl(string url)
		{
			using var client = new WebClient();
			return client.DownloadString(url);
		}

		/// <summary>
		/// Upload form and return data as a string.
		/// </summary>
		/// <param name="url">Url to upload to.</param>
		/// <param name="data">Data to post.</param>
		/// <returns>Web page content.</returns>
		public static string PostFormToUrl(string url, NameValueCollection data)
		{
			using var client = new WebClient();
			return Encoding.UTF8.GetString(client.UploadValues(url, "POST", data));
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
			// creates a SHA256 context
			using SHA256 sha256Hash = SHA256.Create();

			// ComputeHash - returns byte array
			return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
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

		public string GetRedirectUri()
		{
			return redirectUri;
		}
	}
}
