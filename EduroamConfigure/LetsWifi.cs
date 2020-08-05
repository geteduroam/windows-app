using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EduroamConfigure
{
	// https://github.com/geteduroam/lets-wifi
	public class LetsWifi
	{
		// tokens to access API, valid for a small time window
		private static string AccessToken;
		private static string AccessTokenType;
		private static DateTime AccessTokenValidUntill;

		// Persisted state

		private static string ProfileID
		{ get => PersistingStore.LetsWifiEndpoints?.profileId; }
		private static Uri TokenEndpoint // requires either authorization code or refresh token, provides access token
		{ get => PersistingStore.LetsWifiEndpoints?.tokenEndpoint; }
		private static Uri EapEndpoint // requires an access token
		{ get => PersistingStore.LetsWifiEndpoints?.eapEndpoint; }
		private static string RefreshToken // single-use token to refresh the access token (assume Bearer?)
		{
			get => PersistingStore.LetsWifiRefreshToken;
			set => PersistingStore.LetsWifiRefreshToken = value;
		}

		// interface

		public static bool CanRefresh
		{
			get => TokenEndpoint != null
				&& RefreshToken != null;
		}

		public static void WipeTokens()
		{
			RefreshToken = null;
			PersistingStore.LetsWifiEndpoints = null;
		}

		public static bool AuthorizeAccess(IdentityProviderProfile profile, string authorizationCode, string codeVerifier, Uri redirectUri)
		{
			_ = profile ?? throw new ArgumentNullException(paramName: nameof(profile));
			_ = authorizationCode ?? throw new ArgumentNullException(paramName: nameof(authorizationCode));
			_ = codeVerifier ?? throw new ArgumentNullException(paramName: nameof(codeVerifier));

			WipeTokens();

			if (string.IsNullOrEmpty(profile.token_endpoint)) return false;
			if (string.IsNullOrEmpty(profile.eapconfig_endpoint)) return false;

			Uri tokenEndpoint = new Uri(profile.token_endpoint);
			Uri eapEndpoint = new Uri(profile.eapconfig_endpoint);

			// Parameters to get access tokens form lets-wifi
			var tokenPostData = new NameValueCollection() {
				{ "grant_type", "authorization_code" },
				{ "code", authorizationCode },
				{ "redirect_uri", redirectUri?.ToString() },
				{ "client_id", OAuth.clientId },
				{ "code_verifier", codeVerifier }
			};

			// downloads json file from url as string
			var tokenJson = PostForm(tokenEndpoint, tokenPostData);

			// process response
			var success = SetAccessTokensFromJson(tokenJson);

			// Persist location
			if (success)
				PersistingStore.LetsWifiEndpoints = (profile.Id, tokenEndpoint, eapEndpoint);

			return success;
		}

		private static bool SetAccessTokensFromJson(string jsonResponse)
		{
			// Parse json response to retrieve authorization tokens
			string accessToken;
			string accessTokenType;
			string refreshToken;
			int? accessTokenExpiresIn;

			try
			{
				JObject tokenJson = JObject.Parse(jsonResponse);

				accessToken = tokenJson["access_token"].ToString(); // token to retrieve EAP config
				accessTokenType = tokenJson["token_type"].ToString(); // Usually "Bearer", a http authorization scheme
				accessTokenExpiresIn = tokenJson["expires_in"].ToObject<int?>();
				refreshToken = tokenJson["refresh_token"].ToString();
			}
			catch (JsonReaderException)
			{
				return false;
			}

			if (string.IsNullOrEmpty(accessToken)) return false;
			if (string.IsNullOrEmpty(accessTokenType)) return false;
			if (string.IsNullOrEmpty(refreshToken)) return false;
			if (accessTokenExpiresIn == null) return false;

			// if we have enough headroom, have our token expire earlier
			if (accessTokenExpiresIn.Value > 60)
				accessTokenExpiresIn -= 10; // reduces change for error

			AccessToken = accessToken;
			AccessTokenType = accessTokenType;
			AccessTokenValidUntill = DateTime.Now.AddSeconds(accessTokenExpiresIn.Value);
			RefreshToken = refreshToken;

			return true;
		}

		public static bool RefreshTokens()
		{
			if (!CanRefresh) return false;

			var tokenFormData = new NameValueCollection() {
				{ "grant_type", "refresh_token" },
				{ "refresh_token", RefreshToken },
				{ "client_id", OAuth.clientId },
			};

			// downloads json file from url as string
			var tokenJson = PostForm(TokenEndpoint, tokenFormData);

			// process response
			return SetAccessTokensFromJson(tokenJson);
		}

		public static EapConfig DownloadEapConfig()
		{
			if (EapEndpoint == null) return null;
			if (DateTime.Now > AccessTokenValidUntill)
				if (!RefreshTokens())
					return null;

			string eapConfigXml;
			try
			{
				// Setup client with authorization token in header
				using var client = new WebClient();
				client.Headers.Add("Authorization", AccessTokenType + " " + AccessToken);

				// download eap config
				eapConfigXml = client.DownloadString(EapEndpoint.ToString() + "?format=eap-metadata"); // TODO: use a uri builder or something
			}
			catch (WebException ex)
			{
				throw new EduroamAppUserError("oauth eapconfig get error",
					userFacingMessage: "Couldn't fetch EAP config file. \nException: " + ex.Message);
			}

			// parse and return
			return EapConfig.FromXmlData(uid: ProfileID, eapConfigXml);
		}


		// internal helpers

		/// <summary>
		/// Upload form and return data as a string.
		/// </summary>
		/// <param name="url">Url to upload to.</param>
		/// <param name="data">Data to post.</param>
		/// <returns>Web page content.</returns>
		private static string PostForm(Uri url, NameValueCollection data)
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
	}
}
