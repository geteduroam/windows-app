using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EduroamConfigure
{
    // https://github.com/geteduroam/lets-wifi
    public static class LetsWifi
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

        /// <summary>
        /// Parse a json response and store the tokens provided
        /// </summary>
        /// <param name="jsonResponse"></param>
        /// <returns>True if the json was valid</returns>
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

                accessToken = tokenJson["access_token"]?.ToString(); // token to retrieve EAP config
                accessTokenType = tokenJson["token_type"]?.ToString(); // Usually "Bearer", a http authorization scheme
                accessTokenExpiresIn = tokenJson["expires_in"]?.ToObject<int?>();
                refreshToken = tokenJson["refresh_token"]?.ToString();
            }
            catch (JsonReaderException)
            {
                return false; // malformed
            }

            if (string.IsNullOrEmpty(accessToken)) return false;
            if (string.IsNullOrEmpty(accessTokenType)) return false;
            if (accessTokenExpiresIn == null) return false;

            // if we have enough headroom, have our token expire earlier
            if (accessTokenExpiresIn.Value > 60)
                accessTokenExpiresIn -= 10; // reduces chance of error

            AccessToken = accessToken;
            AccessTokenType = accessTokenType;
            AccessTokenValidUntill = DateTime.Now.AddSeconds(accessTokenExpiresIn.Value);
            RefreshToken = refreshToken;

            return true;
        }

        /// <summary>
        /// Will use the refresh token to request a new access token.
        /// </summary>
        /// <returns></returns>
        public static bool RefreshTokens()
        {
            if (!CanRefresh) return false;

            var tokenFormData = new NameValueCollection() {
                { "grant_type", "refresh_token" },
                { "refresh_token", RefreshToken },
                { "client_id", OAuth.clientId },
            };

            // downloads json file from url as string
            // TODO on background refresh, internet may be offline, but be back in a few seconds; smart retry needed
            try
            {
                var tokenJson = PostForm(TokenEndpoint, tokenFormData);

                // process response
                return SetAccessTokensFromJson(tokenJson);
            }
            catch (EduroamAppUserError)
            {
                // TODO log the error somewhere
                return false;
            }
        }

        /// <summary>
        /// Will usually request the LetsWifi endpoint to issue a client certificate
        /// </summary>
        /// <returns>EapConfig with client</returns>
        public static EapConfig RequestAndDownloadEapConfig()
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
            return EapConfig.FromXmlData(profileId: ProfileID, eapConfigXml, isOauth: true);
        }

        /// <summary>
        /// Either refreshes the stored Eapconfig XML (for TTLS and PEAP), or requests
        /// a new EAP config (TLS) from LetsWifi using the refresh token, then installs it.
        /// </summary>
        /// <param name="force">Wether to force a reinstall even if the current certificate still is valid for quote some time</param>
        /// <returns>An enum describing the result</returns>
        public static RefreshResponse RefreshAndInstallEapConfig(bool force = false, bool onlyLetsWifi = false)
        {
            // if LetsWifi is not set up:
            // Download a new EapConfig xml if we already have one stored
            var profileId = PersistingStore.IdentityProvider?.ProfileId;
            if (!onlyLetsWifi
                && !PersistingStore.IsRefreshable
                && PersistingStore.IsReinstallable
                && !string.IsNullOrEmpty(profileId))
            {
                using var discovery = new IdentityProviderDownloader();
                discovery.LoadProviders(useGeodata: false);
                var xml = discovery.DownloadEapConfig(profileId).XmlData;
                PersistingStore.IdentityProvider = PersistingStore.IdentityProvider
                    ?.WithEapConfigXml(xml);
                return RefreshResponse.UpdatedEapXml;
            }

            // otherwise, we try to refresh through LetsWifi

            // Check if we can, at all:
            if (!PersistingStore.IsRefreshable)
                return RefreshResponse.NotRefreshable;

            // should never be null since the check above was successfull
            var profileInfo = PersistingStore.IdentityProvider
                ?? throw new NullReferenceException(nameof(PersistingStore.IdentityProvider) + " was null");

            // check if within 2/3 of the client certificate lifespan
            if (profileInfo.NotBefore != null && profileInfo.NotAfter != null)
            {
                var d1 = profileInfo.NotBefore.Value;
                var d2 = DateTime.Now;
                var d3 = profileInfo.NotAfter.Value;

                // if we are not a least 2/3 into the validity period (let's encrypt convention)
                if (d1.Add(TimeSpan.FromTicks((d3 - d1).Ticks * 2 / 3)) > d2)
                    if (!force)
                        return RefreshResponse.StillValid; // prefer not to issue a new cert
            }

            // this is done automatically by RequestAndDownloadEapConfig, but we do it here for the result code.
            if (!RefreshTokens())
                return RefreshResponse.AccessDenied; // TODO: requires user intervention, make user reconnect

            var eapConfig = RequestAndDownloadEapConfig();
            if (eapConfig == null)
                return RefreshResponse.Failed;

            foreach (var rootCA in ConnectToEduroam.EnumerateCAInstallers(eapConfig))
                if (!rootCA.IsInstalled)
                    return RefreshResponse.NewRootCaRequired; // TODO: requires user intervention, make user reconnect

            // reinstall the same authmethod as last time
            var installer = ConnectToEduroam
                .InstallEapConfig(eapConfig)
                .Where(installer => profileInfo.EapTypeSsid.outer == installer.AuthMethod.EapType)
                .Where(installer => profileInfo.EapTypeSsid.inner == installer.AuthMethod.InnerAuthType)
                .FirstOrDefault();

            // should never fail, since we abort if CA installations are needed
            if (!installer.InstallCertificates())
                return RefreshResponse.Failed;

            // Should only fail if the WLAN service is unavailable (no wireless NIC)
            if (!installer.InstallWLANProfile()) // TODO: currently does not remove the old ones. in the case where ssid was removed from eap config, it will be left stale with an invalid client certificate thumbprint
                return RefreshResponse.Failed;

            // remove the old client certificates installed by us
            using var clientCert = installer.AuthMethod.ClientCertificateAsX509Certificate2();
            var oldClientCerts = CertificateStore.EnumerateInstalledCertificates()
                .Where(cert => cert.installedCert.StoreName == StoreName.My)
                .Where(cert => cert.cert.Thumbprint != clientCert.Thumbprint);
            foreach ((var cert, var installedCert) in oldClientCerts)
                CertificateStore.UninstallCertificate(cert, installedCert.StoreName, installedCert.StoreLocation);
            // TODO: This codeblob above could be reworked into uninstalling only expired certificates, such a helper functiong in CertificateStore would be nice

            // TODO: handle the case where the new installed certificate is not valid yet
            // possible solution is to only automatically refresh at night /shrug

            return RefreshResponse.Success;
        }

        public enum RefreshResponse
        {
            /// <summary>
            /// All is good chief
            /// </summary>
            Success,

            /// <summary>
            /// If the profile was not refreshibly through LetsWifi, but
            /// instead managed to update the profile for a Reinstall
            /// </summary>
            UpdatedEapXml,

            /// <summary>
            /// The user has to install some new root certificates.
            /// User intevention is required
            /// </summary>
            NewRootCaRequired,

            /// <summary>
            /// The refresh token was denied. the user has to reauthenticate
            /// </summary>
            AccessDenied,

            /// <summary>
            /// There is no need to refresh the EAP profile, since it is still valid for quite some time
            /// </summary>
            StillValid,

            /// <summary>
            /// The installed profile is not refreshable
            /// </summary>
            NotRefreshable,

            /// <summary>
            /// Something failed
            /// </summary>
            Failed,
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
