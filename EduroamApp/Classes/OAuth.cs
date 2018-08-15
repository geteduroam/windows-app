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

namespace EduroamApp
{
	class OAuth
	{
		public static void BrowserAuthenticate(string baseUrl)
		{
			string letsWifiHtml;
			// downloads html file from url as string
			using (WebClient client = new WebClient())
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

			// gets a decoded json file with authorization endpoint
			var authEndpointJson = JObject.Parse(jsonString);
			string authEndpoint = authEndpointJson["authorization_endpoint"].ToString();

			// sets authorization uri parameters
			string responseType = "code";
			string codeChallengeMethod = "S256";
			string scope = "eap-metadata";
			string codeVerifier = Base64UrlEncode(GenerateCodeChallengeBase());
			string codeChallenge = Base64UrlEncode(HashWithSHA256(codeVerifier));
			string redirectUri = "http://localhost:8080/";
			string clientId = "f817fbcc-e8f4-459e-af75-0822d86ff47a";
			string state = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20); // random alphanumeric string

			string authUri = CreateUri(authEndpoint, responseType, codeChallengeMethod, scope, codeChallenge, redirectUri, clientId, state);

			string responseUrl = WebServer.NonblockingListener(redirectUri, authUri);

			if (!string.IsNullOrEmpty(responseUrl))
			{
				MessageBox.Show("Nice \n\n" + responseUrl);
			}
		}


		/// <summary>
		/// Gets json and decodes it from base64.
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
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

		private static byte[] GenerateCodeChallengeBase()
		{
			var salt = new byte[32];
			using (var random = new RNGCryptoServiceProvider())
			{
				random.GetNonZeroBytes(salt);
			}
			return salt;
		}

		private static string Base64UrlEncode(byte[] arg)
		{
			string s = Convert.ToBase64String(arg); // Regular base64 encoder
			s = s.Split('=')[0]; // Remove any trailing '='s
			s = s.Replace('+', '-'); // 62nd char of encoding
			s = s.Replace('/', '_'); // 63rd char of encoding
			return s;
		}

		private static byte[] HashWithSHA256(string dataString)
		{
			// Create a SHA256
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array
				return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));
			}
		}

		private static string CreateUri(string mainUri, string responseType, string codeChallengeMethod, string scope, string codeChallenge, string redirectUri, string clientId, string state)
		{
			return
				mainUri + "?"
						+ "response_type=" + responseType
						+ "&code_challenge_method=" + codeChallengeMethod
						+ "&scope=" + scope
						+ "&code_challenge=" + codeChallenge
						+ "&redirect_uri=" + redirectUri
						+ "&client_id=" + clientId
						+ "&state=" + state;
		}
	}
}
