using System.Xml;
using System.Xml.Linq;

namespace EduRoam.Connect.Eap
{
    /// <summary>
    /// Stores information found in an EAP-config file.
    /// </summary>
    public partial class EapConfig
    {
        #region Properties

        public bool IsOauth { get; set; } // TODO: Setter used for scaffolding to PersistenStorage, need better solution
        public string? ProfileId { get; set; } // TODO: Setter used for scaffolding to PersistenStorage, need better solution
        public List<AuthenticationMethod> AuthenticationMethods { get; }
        public List<CredentialApplicability> CredentialApplicabilities { get; }
        public ProviderInfo InstitutionInfo { get; }
        public string RawOriginalEapConfigXmlData { get; }

        #endregion

        #region Helpers

        public IEnumerable<string> SSIDs { get => this.CredentialApplicabilities.Where((c) => !string.IsNullOrWhiteSpace(c.Ssid)).Select((c) => c.Ssid!); }

        [Obsolete("Should this be used?")]
        public IEnumerable<string> ConsortiumOids { get => this.CredentialApplicabilities.Where((c) => !string.IsNullOrWhiteSpace(c.ConsortiumOid)).Select((c) => c.ConsortiumOid!); }

        #endregion

        #region Constructor

        private EapConfig(
            List<AuthenticationMethod> authenticationMethods,
            List<CredentialApplicability> credentialApplicabilities,
            ProviderInfo institutionInfo,
            string eapConfigXmlData)
        {
            this.AuthenticationMethods = authenticationMethods.Select(authMethod => authMethod.WithEapConfig(this)).ToList();
            this.CredentialApplicabilities = credentialApplicabilities;
            this.InstitutionInfo = institutionInfo;
            this.RawOriginalEapConfigXmlData = eapConfigXmlData;
        }

        #endregion

        /// <summary>
        /// Creates a new EapConfig object from EAP config xml data
        /// </summary>
        /// <param name="eapConfigXmlData">EAP config XML as string</param>
        /// <returns>EapConfig object</returns>
        /// <exception cref="XmlException">Parsing <paramref name="eapConfigXmlData"/> failed</exception>
        public static EapConfig FromXmlData(string eapConfigXmlData)
        {
            // XML format Documentation:
            // Current:  https://github.com/GEANT/CAT/blob/master/devices/eap_config/eap-metadata.xsd
            // Outdated: https://tools.ietf.org/id/draft-winter-opsawg-eap-metadata-00.html

            // TODO: validate the file first. use schema?
            // TODO: add a test on this function using fuzzing accoring to schema

            static Func<XElement, bool> nameIs(string name) => // shorthand lambda
                element => element.Name.LocalName == name;

            // load the XML file into a XElement object
            XElement eapConfigXml;
            try
            {
                eapConfigXml = XElement.Parse(eapConfigXmlData);
            }
            catch (XmlException)
            {
                throw; // explicitly show that XmlException can be thrown here
            }
            /*
			foreach (XElement eapIdentityProvider in eapConfigXml.Descendants().Where(nameIs("EAPIdentityProvider")))
			{
				// NICE TO HAVE: yield return from this
			}
			*/

            // create a new empty list for authentication methods
            var authMethods = new List<AuthenticationMethod>();

            // iterate over all AuthenticationMethods elements from xml
            foreach (var authMethodXml in eapConfigXml.Descendants().Where(nameIs("AuthenticationMethod")))
            {
                var serverSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ServerSideCredential"));
                var clientSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ClientSideCredential"));

                // get EAP method type
                var eapType = (EapType)(int)authMethodXml
                    .Elements().First(nameIs("EAPMethod"))
                    .Elements().First(nameIs("Type"));

                var innerAuthType = (InnerAuthType?)(int?)authMethodXml
                    .Elements().FirstOrDefault(nameIs("InnerAuthenticationMethod"))
                    ?.Descendants().FirstOrDefault(nameIs("Type"))
                    ?? InnerAuthType.None;

                // ServerSideCredential

                // get list of strings of CA certificates
                var serverCAs = serverSideCredentialXml?
                    .Elements().Where(nameIs("CA")) // TODO: <CA format="X.509" encoding="base64"> is assumed, schema does not enforce this
                    .Select(xElement => (string)xElement)
                    .ToList();

                // get list of strings of server IDs
                var serverNames = serverSideCredentialXml?
                    .Elements().Where(nameIs("ServerID"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // ClientSideCredential

                // Preset credentials
                var clientUserName = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("UserName"));
                var clientPassword = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Password"));
                var clientCert = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("ClientCertificate")); // TODO: <ClientCertificate format="PKCS12" encoding="base64"> is assumed
                var clientCertPasswd = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Passphrase"));

                // inner/outer identity
                var clientOuterIdentity = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("OuterIdentity"));
                var clientInnerIdentitySuffix = (string?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
                var clientInnerIdentityHint = (bool?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentityHint")) ?? false;

                // create new authentication method object and adds it to list
                authMethods.Add(new AuthenticationMethod(
                    eapType,
                    innerAuthType,
                    serverCAs ?? new List<string>(),
                    serverNames ?? new List<string>(),
                    clientUserName,
                    clientPassword,
                    clientCert,
                    clientCertPasswd,
                    clientOuterIdentity,
                    clientInnerIdentitySuffix,
                    clientInnerIdentityHint
                ));
            }

            // create a new empty list for authentication methods
            var credentialApplicabilities = new List<CredentialApplicability>();

            foreach (var credentialApplicabilityXml in eapConfigXml.Descendants().First(nameIs("CredentialApplicability")).Elements())
            {
                credentialApplicabilities.Add(credentialApplicabilityXml.Name.LocalName switch
                {
                    "IEEE80211" =>
                        CredentialApplicability.IEEE80211(
                            (string?)credentialApplicabilityXml?.Elements().FirstOrDefault(nameIs("SSID")),
                            (string?)credentialApplicabilityXml?.Elements().FirstOrDefault(nameIs("ConsortiumOID")),
                            (string?)credentialApplicabilityXml?.Elements().FirstOrDefault(nameIs("MinRSNProto"))
                        ),
                    "IEEE8023" =>
                        CredentialApplicability.IEEE8023(
                            (string?)credentialApplicabilityXml?.Elements().FirstOrDefault(nameIs("NetworkID"))
                        ),
                    _ => throw new NotImplementedException(),
                });
            }

            // get logo and identity element
            var logoElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderLogo"));
            var eapIdentityElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("EAPIdentityProvider")); // NICE TO HAVE: update this if the yield return above gets used

            // get institution ID from identity element
            var instId = (string?)eapIdentityElement?.Attribute("ID");

            // get provider's logo as base64 encoded string and its mime-type
            var logoData = Convert.FromBase64String((string?)logoElement ?? "");
            var logoMimeType = (string?)logoElement?.Attribute("mime");

            // Read ProviderInfo attributes:
            var providerInfoXml = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderInfo"));

            var displayName = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("DisplayName"));
            var description = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Description"));
            var emailAddress = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("EmailAddress"));
            var webAddress = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("WebAddress"));
            var phone = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("Phone"));
            var termsOfUse = (string?)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("TermsOfUse"));

            // Read coordinates
            ValueTuple<double, double>? location = null;
            if (providerInfoXml?.Elements().Where(nameIs("ProviderLocation")).Any() ?? false)
            {
                location = (
                    (double)providerInfoXml.Descendants().First(nameIs("Latitude")),
                    (double)providerInfoXml.Descendants().First(nameIs("Longitude"))
                );
            }

            // create EapConfig object and adds the info
            return new EapConfig(
                authMethods,
                credentialApplicabilities,
                new ProviderInfo(
                    displayName ?? string.Empty,
                    description ?? string.Empty,
                    logoData,
                    logoMimeType ?? string.Empty,
                    emailAddress ?? string.Empty,
                    webAddress ?? string.Empty,
                    phone ?? string.Empty,
                    instId ?? string.Empty,
                    termsOfUse ?? string.Empty,
                    location),
                eapConfigXmlData
            );
        }

        /// <summary>
        /// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
        /// Refer to frmSummary.InstallEapConfig to see how to use it (TODO: actually explain when finalized)
        /// </summary>
        /// <param name="eapConfig">EapConfig object</param>
        /// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
        public IEnumerable<AuthenticationMethod> SupportedAuthenticationMethods
        {
            get => this.AuthenticationMethods.Where(EduRoamNetwork.IsAuthMethodSupported);
        }

        /// <summary>
        /// Used to determine if an eapconfig has enough info for the ProfileOverview page to show
        /// </summary>
        public bool HasInfo => !string.IsNullOrEmpty(this.InstitutionInfo.WebAddress)
                               || !string.IsNullOrEmpty(this.InstitutionInfo.EmailAddress)
                               || !string.IsNullOrEmpty(this.InstitutionInfo.Description)
                               || !string.IsNullOrEmpty(this.InstitutionInfo.Phone)
                               || !string.IsNullOrEmpty(this.InstitutionInfo.TermsOfUse);

        /// <summary>
        /// If this returns true, then the user must provide the login credentials
        /// when installing with ConnectToEduroam or EduroamNetwork
        /// </summary>
        public bool NeedsLoginCredentials
        {
            get => this.AuthenticationMethods.Any(authMethod => authMethod.NeedsLoginCredentials);
        }

        /// <summary>
        /// If this is true, then you must provide a
        /// certificate file and add it with this.AddClientCertificate
        /// </summary>
        public bool NeedsClientCertificate
        {
            get => this.AuthenticationMethods
                .Any(authMethod => authMethod.NeedsClientCertificate);
        }

        /// <summary>
        /// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
        /// Add this passphrase with this.AddClientCertificatePassphrase
        /// </summary>
        public bool NeedsClientCertificatePassphrase
        {
            get => this.AuthenticationMethods
                .Any(authMethod => authMethod.NeedsClientCertificatePassphrase);
        }

        /// <summary>
        /// Determine if this EapConfig needs the anonymous ident to have the same realm as the username
        /// This is not enforced, the realm is simply dropped if needed, but this variable can be used to warn the user if the anonymous ident is modified
        /// Empty string means the username is required to not have a realm, null means that no realm is required
        /// </summary>
        public string? RequiredAnonymousIdentRealm
        {
            get => !string.IsNullOrEmpty(this.SupportedAuthenticationMethods.First().ClientOuterIdentity)
                && this.SupportedAuthenticationMethods.First().EapType == EapType.PEAP
                && this.SupportedAuthenticationMethods.First().InnerAuthType == InnerAuthType.EAP_MSCHAPv2
                ? this.SupportedAuthenticationMethods.First().ClientOuterIdentity?.Contains('@') ?? false
                    ? this.SupportedAuthenticationMethods.First().ClientOuterIdentity!.Substring(this.SupportedAuthenticationMethods.First().ClientOuterIdentity!.IndexOf('@'))
                    : ""
                : null;
        }

        /// <summary>
        /// Reads and adds the user certificate to be installed along with the wlan profile
        /// </summary>
        /// <param name="filePath">path to the certificate file in question. PKCS12</param>
        /// <param name="passphrase">the passphrase to the certificate file in question</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        /// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
        internal EapConfig WithClientCertificate(string certificatePath, string? certificatePassphrase = null)
        {
            var authMethods = this.AuthenticationMethods.Select(authMethod => authMethod.WithClientCertificate(certificatePath, certificatePassphrase))
                .Where(authMethod => authMethod != null)
                .Select(authMethod => authMethod!);

            if (!authMethods.Any())
            {
                throw new ArgumentException("No authentication method can accept the client certificate");
            }

            return new EapConfig(
                authMethods.ToList(),
                this.CredentialApplicabilities,
                this.InstitutionInfo,
                this.RawOriginalEapConfigXmlData
            );
        }

        /// <summary>
        /// Sets the passphrase to use when derypting the certificate bundle.
        /// Will only be stored if valid.
        /// </summary>
        /// <param name="passphrase">the passphrase to the certificate</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        /// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
        internal EapConfig WithClientCertificatePassphrase(string certificatePassphrase)
        {
            var authMethods = this.AuthenticationMethods.Select(authMethod => authMethod.WithClientCertificatePassphrase(certificatePassphrase))
                .Where(authMethod => authMethod != null)
                .Select(authMethod => authMethod!);

            if (!authMethods.Any())
            {
                throw new ArgumentException("No authentication accepts the passphrase");
            }

            return new EapConfig(
                authMethods.ToList(),
                this.CredentialApplicabilities,
                this.InstitutionInfo,
                this.RawOriginalEapConfigXmlData
            );
        }

        /// <summary>
        /// Sets the username/password for inner auth.
        /// </summary>
        /// <param name="username">The username for inner auth</param>
        /// <param name="password">The passpword for inner auth</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        /// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
        internal EapConfig WithLoginCredentials(string username, string password)
        {
            var authMethods = this.AuthenticationMethods.Select(authMethod => authMethod.WithLoginCredentials(username, password))
                .Where(authMethod => authMethod != null)
                .Select(authMethod => authMethod!);

            if (!authMethods.Any())
            {
                throw new ArgumentException("No authentication accepts the passphrase");
            }

            return new EapConfig(
                authMethods.ToList(),
                this.CredentialApplicabilities,
                this.InstitutionInfo,
                this.RawOriginalEapConfigXmlData
            );
        }

        /// <summary>
        /// goes through all the AuthenticationMethods in this config and tries to reason about a correct method
        /// the suffix may be null or empty, null means no realm check must be done,
        /// empty means that any realm is valid but a realm must be provided
        /// https://github.com/GEANT/CAT/blob/master/tutorials/MappingCATOptionsIntoSupplicantConfig.md#verify-user-input-to-contain-realm-suffix-checkbox
        /// </summary>
        /// <returns>A ValueTuple with the inner identity suffix and hint</returns>
        public (string? suffix, bool hint) GetClientInnerIdentityRestrictions()
        {
            var hint = this.AuthenticationMethods
                .All(authMethod => authMethod.ClientInnerIdentityHint);
            var suffi = this.AuthenticationMethods
                .Select(authMethod => authMethod.ClientInnerIdentitySuffix)
                .ToList();

            string? suffix = null;
            if (suffi.Any())
            {
                var first = suffi.First();
                if (suffi.All(suffix => suffix == first))
                {
                    suffix = first;
                }
            }
            return (suffix, hint);
        }

    }

}
