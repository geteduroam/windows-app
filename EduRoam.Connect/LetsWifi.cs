using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Install;
using EduRoam.Connect.Store;
using EduRoam.Localization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect
{
    // https://github.com/geteduroam/lets-wifi
    internal class LetsWifi
    {
        // tokens to access API, valid for a small time window
        private string? AccessToken { get; set; }

        private string? AccessTokenType { get; set; }

        private DateTime? AccessTokenValidUntill { get; set; }

        private readonly BaseConfigStore store;

        private LetsWifi()
        {
            this.store = new RegistryStore();
        }

        private static LetsWifi instance = new LetsWifi();

        internal static LetsWifi Instance => instance;

        private string? ProfileID { get => this.store.WifiEndpoint?.ProfileId; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// requires either authorization code or refresh token, provides access token
        /// </remarks>
        private Uri? TokenEndpoint { get => this.store.WifiEndpoint?.TokenEndpoint; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// requires an access token
        /// </remarks>
        private Uri? EapEndpoint { get => this.store.WifiEndpoint?.EapEndpoint; }

        /// <summary>
        /// single-use token to refresh the access token (Bearer header)
        /// </summary>
        private string? RefreshToken
        {
            get => this.store.WifiRefreshToken;
            set => this.store.UpdateWifiRefreshToken(value);
        }

        // interface
        public string? VersionNumber
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly()!;
                foreach (var attrs in assembly.CustomAttributes)
                {
                    if (attrs.AttributeType.Name == "AssemblyFileVersionAttribute")
                    {
                        foreach (var attr in attrs.ConstructorArguments)
                        {
                            if (attr.Value is string versionNumber)
                            {
                                return versionNumber;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public bool CanRefresh
        {
            get => this.TokenEndpoint != null
                && this.RefreshToken != null;
        }

        public static void WipeTokens()
        {
            RegistryStore.Instance.ClearWifiRefreshToken();
            RegistryStore.Instance.ClearWifiEndpoint();
        }

        public async Task<bool> AuthorizeAccess(IdentityProviderProfile profile, string authorizationCode, string codeVerifier, Uri redirectUri)
        {
            _ = profile ?? throw new ArgumentNullException(paramName: nameof(profile));
            _ = authorizationCode ?? throw new ArgumentNullException(paramName: nameof(authorizationCode));
            _ = codeVerifier ?? throw new ArgumentNullException(paramName: nameof(codeVerifier));

            WipeTokens();

            if (string.IsNullOrEmpty(profile.TokenEndpoint))
            {
                return false;
            }

            if (string.IsNullOrEmpty(profile.EapConfigEndpoint))
            {
                return false;
            }

            var tokenEndpoint = new Uri(profile.TokenEndpoint);
            var eapEndpoint = new Uri(profile.EapConfigEndpoint);

            // Parameters to get access tokens form lets-wifi
            var tokenPostData = new NameValueCollection() {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri?.ToString() },
                { "client_id", OAuth.clientId },
                { "code_verifier", codeVerifier }
            };

            // downloads json file from url as string
            string tokenJson;
            try
            {
                tokenJson = await IdentityProviderDownloader.PostForm(tokenEndpoint, tokenPostData);
            }
            catch (HttpRequestException ex)
            {
                throw new EduroamAppUserException("oauth post error",
                    userFacingMessage: string.Format(Resources.ErrorCannotFetchTokens, ex.Message));
            }

            // process response
            try
            {
                this.SetAccessTokensFromJson(tokenJson);
                this.store.UpdateWifiEndpoint(new WifiEndpoint(profile.Id, tokenEndpoint, eapEndpoint));
            }
            catch (ApiParsingException) { return false; }

            return true;
        }

        /// <summary>
        /// Parse a json response and store the tokens provided
        /// </summary>
        /// <param name="jsonResponse"></param>
        /// <exception cref="ApiParsingException">Misformed JSON or missing requird fields</exception>
        private void SetAccessTokensFromJson(string jsonResponse)
        {
            // Parse json response to retrieve authorization tokens
            string? accessToken;
            string? accessTokenType;
            string? refreshToken;
            int? accessTokenExpiresIn;

            JObject tokenJson;
            try
            {
                tokenJson = JObject.Parse(jsonResponse);
            }
            catch (JsonReaderException e)
            {
                throw new ApiParsingException("Unable to parse JSON in OAuth response", e);
            }

            accessToken = tokenJson["access_token"]?.ToString(); // token to retrieve EAP config
            accessTokenType = tokenJson["token_type"]?.ToString(); // Usually "Bearer", a http authorization scheme
            accessTokenExpiresIn = tokenJson["expires_in"]?.ToObject<int?>();
            refreshToken = tokenJson["refresh_token"]?.ToString();

            if (string.IsNullOrEmpty(accessToken)
                || string.IsNullOrEmpty(accessTokenType)
                || accessTokenExpiresIn == null)
            {
                throw new ApiParsingException("Missing required fields in OAuth response");
            }

            // if we have enough headroom, have our token expire earlier
            if (accessTokenExpiresIn.Value > 60)
            {
                accessTokenExpiresIn -= 10; // reduces chance of error
            }

            this.AccessToken = accessToken;
            this.AccessTokenType = accessTokenType;
            this.AccessTokenValidUntill = DateTime.Now.AddSeconds(accessTokenExpiresIn.Value);
            this.RefreshToken = refreshToken;
        }

        /// <summary>
        /// Will use the refresh token to request a new access token.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshTokens()
        {
            if (!this.CanRefresh)
            {
                return false;
            }

            var tokenFormData = new NameValueCollection() {
                { "grant_type", "refresh_token" },
                { "refresh_token", this.RefreshToken },
                { "client_id", OAuth.clientId },
            };

            // downloads json file from url as string
            // TODO on background refresh, internet may be offline, but be back in a few seconds; smart retry needed
            try
            {
                var tokenJson = await IdentityProviderDownloader.PostForm(this.TokenEndpoint, tokenFormData, new string[] { "application/json" });

                // process response
                this.SetAccessTokensFromJson(tokenJson);
                return true;
            }
            catch (ApiException e)
            {
                Debug.Print(e.ToString());
                return false;
            }
            catch (HttpRequestException e)
            {
                Debug.Print(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Will usually request the LetsWifi endpoint to issue a client certificate
        /// </summary>
        /// <returns>EapConfig with client</returns>
        /// <exception cref="XmlException"></exception>
        public async Task<EapConfig?> RequestAndDownloadEapConfig()
        {
            if (this.EapEndpoint == null)
            {
                return null;
            }

            if (DateTime.Now > this.AccessTokenValidUntill)
            {
                if (false == await this.RefreshTokens())
                {
                    return null;
                }
            }

            if (this.AccessTokenType != "Bearer")
            {
                throw new InvalidOperationException("Expected token_type Bearer but got " + this.AccessTokenType);
            }

            var eapConfig = await IdentityProviderDownloader.DownloadEapConfig(this.EapEndpoint, this.AccessToken);
            eapConfig.ProfileId = this.ProfileID;
            eapConfig.IsOauth = true;

            return eapConfig;
        }

        /// <summary>
        /// Either refreshes the stored Eapconfig XML (for TTLS and PEAP), or requests
        /// a new EAP config (TLS) from LetsWifi using the refresh token, then installs it.
        /// </summary>
        /// <param name="force">Wether to force a reinstall even if the current certificate still is valid for quote some time</param>
        /// <returns>An enum describing the result</returns>
        /// <exception cref="ApiParsingException">JSON cannot be deserialized</exception>
        public async Task<RefreshResponse> RefreshAndInstallEapConfig(bool force = false, bool onlyLetsWifi = false)
        {
            // if LetsWifi is not set up:
            // Download a new EapConfig xml if we already have one stored
            var profileId = this.store.IdentityProvider?.ProfileId;

            if (!onlyLetsWifi
                && !this.store.IsRefreshable
                && this.store.IsReinstallable
                && !string.IsNullOrEmpty(profileId))
            {
                try
                {
                    using var discovery = new IdentityProviderDownloader();
                    var xml = (await discovery.DownloadEapConfig(profileId)).RawOriginalEapConfigXmlData;

                    var identityProvider = this.store.IdentityProvider?.WithEapConfigXml(xml);
                    this.store.UpdateIdentity(identityProvider);
                    return RefreshResponse.UpdatedEapXml;
                }
                catch (ApiParsingException e)
                {
                    // Must never happen, because if the discovery is reached,
                    // it must be parseable. If it happens anyway, SCREAM!
                    Debug.Print(e.ToString());
                    throw;
                }
                catch (ApiUnreachableException e)
                {
                    Debug.Print(e.ToString());
                    return RefreshResponse.Failed;
                }
            }

            // otherwise, we try to refresh through LetsWifi

            // Check if we can, at all:
            if (!this.store.IsRefreshable)
            {
                return RefreshResponse.NotRefreshable;
            }

            // should never be null since the check above was successfull
            var profileInfo = this.store.IdentityProvider
                ?? throw new NullReferenceException(nameof(this.store.IdentityProvider) + " was null");

            // check if within 2/3 of the client certificate lifespan
            if (!force && profileInfo.NotBefore != null && profileInfo.NotAfter != null)
            {
                var d1 = profileInfo.NotBefore.Value;
                var d2 = DateTime.Now;
                var d3 = profileInfo.NotAfter.Value;

                // if we are not a least 2/3 into the validity period (let's encrypt convention)
                if (d1.Add(TimeSpan.FromTicks((d3 - d1).Ticks * 2 / 3)) > d2)
                {
                    return RefreshResponse.StillValid; // prefer not to issue a new cert
                }
            }

            // this is done automatically by RequestAndDownloadEapConfig, but we do it here for the result code.
            if (false == await this.RefreshTokens())
            {
                return RefreshResponse.AccessDenied; // TODO: requires user intervention, make user reconnect
            }

            var eapConfig = await this.RequestAndDownloadEapConfig();
            if (eapConfig == null)
            {
                return RefreshResponse.Failed;
            }

            foreach (var rootCA in ConnectToEduroam.EnumerateCAInstallers(eapConfig))
            {
                if (!rootCA.IsInstalled)
                {
                    return RefreshResponse.NewRootCaRequired; // TODO: requires user intervention, make user reconnect
                }
            }

            // reinstall the same authmethod as last time
            var installer = eapConfig.SupportedAuthenticationMethods
                .Select(authMethod => new EapAuthMethodInstaller(authMethod))
                .Where(installer => profileInfo.EapTypeSsid?.outer == installer.AuthMethod.EapType)
                .Where(installer => profileInfo.EapTypeSsid?.inner == installer.AuthMethod.InnerAuthType)
                .First();

            // should never fail, since we abort if CA installations are needed
            try { installer.InstallCertificates(); }
            catch (Exception)
            {
                return RefreshResponse.Failed;
            }

            // Should only fail if the WLAN service is unavailable (no wireless NIC)
            try
            {
                installer.InstallWLANProfile(); // TODO: currently does not remove the old ones. in the case where ssid was removed from eap config, it will be left stale with an invalid client certificate thumbprint
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return RefreshResponse.Failed;
            }

            // remove the old client certificates installed by us
            using var clientCert = installer.AuthMethod.ClientCertificateAsX509Certificate2();

            var oldClientCerts = CertificateStore.EnumerateInstalledCertificates()
                .Where(cert => cert.installedCert.StoreName == StoreName.My)
                .Where(cert => cert.cert.Thumbprint != clientCert?.Thumbprint);

            foreach ((var cert, var installedCert) in oldClientCerts)
            {
                CertificateStore.UninstallCertificate(cert, installedCert.StoreName, installedCert.StoreLocation);
            }
            // TODO: This codeblob above could be reworked into uninstalling only expired certificates, such a helper functiong in CertificateStore would be nice

            // TODO: handle the case where the new installed certificate is not valid yet
            // possible solution is to only automatically refresh at night /shrug

            return RefreshResponse.Success;
        }
    }
}
