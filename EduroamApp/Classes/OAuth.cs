using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Device.Location;
using System.Globalization;
using eduOAuth;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Web;

namespace EduroamApp
{
	class OAuth
	{
		public static void BrowserAuthenticate(string baseUrl)
		{
			string letsWifiHtml;
			// downloads html file from url as string
			using (var client = new WebClient())
			{
				letsWifiHtml = client.DownloadString(baseUrl);
			}
			// gets the base64 encoded json containing the authorization endpoint from html
			string jsonString = GetBase64AndDecode(letsWifiHtml);
			// if no json found in html, stop execution
			if (string.IsNullOrEmpty(jsonString))
			{
				MessageBox.Show("HTML doesn't contain authorization endpoint json.");
				return;
			}

			// gets a decoded json file with authorization and token endpoint
			var endpointJson = JObject.Parse(jsonString);
			string authEndpoint = endpointJson["authorization_endpoint"].ToString();
			string tokenEndpoint = endpointJson["token_endpoint"].ToString();
			string generatorEndpoint = endpointJson["generator_endpoint"].ToString();

			// sets authorization uri parameters
			string responseType = "code";
			string codeChallengeMethod = "S256";
			string scope = "eap-metadata";
			string codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase());
			string codeChallenge = Base64UrlEncode(HashWithSHA256(codeVerifier));
			string redirectUri = "http://localhost:8080/";
			string clientId = "f817fbcc-e8f4-459e-af75-0822d86ff47a";
			string state = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20); // random alphanumeric string

			string authUri = CreateAuthEndpointUri(authEndpoint, responseType, codeChallengeMethod, scope, codeChallenge, redirectUri, clientId, state);

			string responseUrl = WebServer.NonblockingListener(redirectUri, authUri);
			string tokenJsonString = "";

			// checks if returned url is not empty
			if (!string.IsNullOrEmpty(responseUrl))
			{
				var responseUri = new Uri(responseUrl);
				string newState = HttpUtility.ParseQueryString(responseUri.Query).Get("state");
				// checks if state has remained, if not cancel operation
				if (newState == state)
				{
					string grantType = "authorization_code";
					string code = HttpUtility.ParseQueryString(responseUri.Query).Get("code");
					string tokenUri = CreateTokenEndpointUri(tokenEndpoint, grantType, code, redirectUri, clientId, codeVerifier);

					try
					{
						// downloads json file from url as string
						using (var client = new WebClient())
						{
							tokenJsonString = client.DownloadString(tokenUri);
						}


					}
					catch (WebException ex)
					{
						MessageBox.Show("Exception: " + ex.Message + "\nCouldn't fetch token json.");
					}

				}
				else
				{
					MessageBox.Show("State from request and response do not match. Aborting operation.");
				}
			}

			// gets a decoded json file with authorization and token endpoint
			var tokenJson = JObject.Parse(tokenJsonString);
			string token = tokenJson["access_token"].ToString();
			string tokenType = tokenJson["token_type"].ToString();

			string finalHtml = "";
			using (var client = new WebClient())
			{
				client.Headers.Add("Authorization", tokenType + " " + token);
				finalHtml = client.DownloadString(generatorEndpoint + "?format=eap-metadata"); // should be POST request
			}

			MessageBox.Show(finalHtml);
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
			string s = Convert.ToBase64String(arg); // Regular base64 encoder
			s = s.Split('=')[0]; // Remove any trailing '='s
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
			// Create a SHA256
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array
				return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
			}
		}

		/// <summary>
		/// Concatenates parameters to create an Authorization Endpoint URI.
		/// </summary>
		/// <param name="authEndpoint">Authorization endpoint.</param>
		/// <param name="responseType">Response type.</param>
		/// <param name="codeChallengeMethod">Code challenge method.</param>
		/// <param name="scope">Scope.</param>
		/// <param name="codeChallenge">Code challenge.</param>
		/// <param name="redirectUri">Redirect URI.</param>
		/// <param name="clientId">Client ID.</param>
		/// <param name="state">State.</param>
		/// <returns>Authorization endpoint URI.</returns>
		private static string CreateAuthEndpointUri(string authEndpoint, string responseType, string codeChallengeMethod, string scope, string codeChallenge, string redirectUri, string clientId, string state)
		{
			return
				authEndpoint
				+ "?response_type=" + responseType
				+ "&code_challenge_method=" + codeChallengeMethod
				+ "&scope=" + scope
				+ "&code_challenge=" + codeChallenge
				+ "&redirect_uri=" + redirectUri
				+ "&client_id=" + clientId
				+ "&state=" + state;
		}

		/// <summary>
		/// Concatenates parameters to create an Token Endpoint URI.
		/// </summary>
		/// <param name="tokenEndpoint">Token endpoint.</param>
		/// <param name="grantType">Grant type.</param>
		/// <param name="code">Code.</param>
		/// <param name="redirectUri">Redirect URI.</param>
		/// <param name="clientId">Client ID.</param>
		/// <param name="codeVerifier">Code verifier.</param>
		/// <returns></returns>
		private static string CreateTokenEndpointUri(string tokenEndpoint, string grantType, string code, string redirectUri, string clientId, string codeVerifier)
		{
			return
				tokenEndpoint
				+ "?grant_type=" + grantType
				+ "&code=" + code
				+ "&redirect_uri=" + redirectUri
				+ "&client_id=" + clientId
				+ "&code_verifier=" + codeVerifier;
		}

	}


}
