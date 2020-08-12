using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Stores information found in an EAP-config file.
    /// </summary>
    public class EapConfig
    {
        // Properties
        public string Uid { get; }
        public List<AuthenticationMethod> AuthenticationMethods { get; }
        public List<CredentialApplicability> CredentialApplicabilities { get; }
        public ProviderInfo InstitutionInfo { get; }

        // Constructor
        private EapConfig(
            string uid,
            List<AuthenticationMethod> authenticationMethods,
            List<CredentialApplicability> credentialApplicabilities,
            ProviderInfo institutionInfo)
        {
            Uid = uid;
            AuthenticationMethods = authenticationMethods;
            CredentialApplicabilities = credentialApplicabilities;
            InstitutionInfo = institutionInfo;

            AuthenticationMethods.ForEach(authMethod =>
            {
                authMethod.EapConfig = this;
            });
        }


        /// <summary>
        /// AuthenticationMethod contains information about client certificates and CAs.
        /// </summary>
        public class AuthenticationMethod
        {
            // Properties
            public EapConfig EapConfig { get; set; } // reference to parent EapConfig
            public EapType EapType { get; }
            public InnerAuthType InnerAuthType { get; }
            public List<string> ServerCertificateAuthorities { get; } // base64 encoded DER certificate
            public List<string> ServerNames { get; }
            public string ClientUserName { get; } // preset inner identity, expect it to have a realm
            public string ClientPassword { get; } // preset outer identity
            public string ClientCertificate { get; private set; } // base64 encoded PKCS12 certificate+privkey bundle
            public string ClientCertificatePassphrase { get; private set; } // passphrase for ^
            public string ClientOuterIdentity { get; } // expect it to have a realm. Also known as: anonymous identity, routing identity
            public string ClientInnerIdentitySuffix { get; } // realm
            public bool ClientInnerIdentityHint { get; } // Wether to disallow subrealms or not (see https://github.com/GEANT/CAT/issues/190)

            // helpers:

            public DateTime? ClientCertificateNotBefore
            {
                get
                {
                    using var cert = ClientCertificateAsX509Certificate2();
                    return cert?.NotBefore;
                }
            }
            public DateTime? ClientCertificateNotAfter
            {
                get
                {
                    using var cert = ClientCertificateAsX509Certificate2();
                    return cert?.NotAfter;
                }
            }


            private byte[] ClientCertificateRaw
            { get => Convert.FromBase64String(ClientCertificate); }
            private bool CertificateIsValid
            {
                get => VerifyCertificateBundle(
                        Convert.FromBase64String(ClientCertificate),
                        ClientCertificatePassphrase);
            }

            /// <summary>
            /// Will point to 'this' if it supports Hotspot2.0,
            /// otherwise points to the first one supports Hotspot2.0 in EapConfig.AuthenticationMethods,
            /// otherwise null.
            ///
            /// This method is somewhat risky, since other authMethods may use other certificates
            /// Ensure you install certificates with ConnectToEduroam.EnumerateCAs()
            /// </summary>
            public AuthenticationMethod Hs2AuthMethod {
                get => ProfileXml.SupportsHs2(this)
                    ? this
                    : EapConfig.AuthenticationMethods
                        .FirstOrDefault(ProfileXml.SupportsHs2);
            }

            /// <summary>
            /// Converts and enumerates CertificateAuthorities as X509Certificate2 objects.
            /// The objects are disposed of when the next object is yielded
            /// </summary>
            public IEnumerable<X509Certificate2> CertificateAuthoritiesAsX509Certificate2()
            {
                foreach (var ca in ServerCertificateAuthorities)
                {
                    X509Certificate2 cert;
                    try
                    {
                        // TODO: find some nice way to ensure these are disposed of properly
                        cert = new X509Certificate2(Convert.FromBase64String(ca));
                    }
                    catch (CryptographicException)
                    {
                        throw new EduroamAppUserError("corrupt certificate",
                            "EAP profile has an malformed or corrupted certificate");
                    }

                    // sets the friendly name of certificate
                    if (string.IsNullOrEmpty(cert.FriendlyName))
                        cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);

                    yield return cert;
                }
            }

            /// <summary>
            /// Converts the client certificate base64 data to a X509Certificate2 object
            /// </summary>
            /// <returns>X509Certificate2 if any. Null if non exist or the passphrase is incorrect</returns>
            public X509Certificate2 ClientCertificateAsX509Certificate2()
            {
                if (string.IsNullOrEmpty(ClientCertificate))
                    return null;

                try
                {
                    var cert = new X509Certificate2(
                        Convert.FromBase64String(ClientCertificate),
                        ClientCertificatePassphrase,
                        X509KeyStorageFlags.PersistKeySet);

                    // sets the friendly name of certificate
                    if (string.IsNullOrEmpty(cert.FriendlyName))
                        cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);

                    return cert;
                }
                catch (CryptographicException ex)
                {
                    if ((ex.HResult & 0xFFFF) == 0x56)
                        return null; // wrong passphrase

                    throw new EduroamAppUserError("corrupt client certificate",
                        "EAP profile has an malformed or corrupted client certificate");
                }
            }

            /// <summary>
            /// User presentable EAP scheme identitifer
            /// </summary>
            public string EapSchemeName()
            {
                if (InnerAuthType != InnerAuthType.None)
                    return EapType.ToString() + "_" + InnerAuthType.ToString();
                return EapType.ToString();
            }

            // methods to check if the authentification method is complete, and methods to mend it

            /// <summary>
            /// If this returns true, then the user must provide the login credentials
            /// when installing with ConnectToEduroam or EduroamNetwork
            /// </summary>
            public bool NeedsLoginCredentials()
            {
                if (UserDataXml.NeedsLoginCredentials(this)) // Auth method expects it
                {
                    if (string.IsNullOrEmpty(ClientUserName) || string.IsNullOrEmpty(ClientUserName)) // we don't already have them
                       return true;
                }
                return false;
            }

            /// <summary>
            /// If this is true, then you must provide a
            /// certificate file and add it with this.AddClientCertificate
            /// </summary>
            public bool NeedsClientCertificate()
            {
                if (UserDataXml.NeedsLoginCredentials(this)) return false;
                return string.IsNullOrEmpty(ClientCertificate);
            }

            /// <summary>
            /// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
            /// Add this passphrase with this.AddClientCertificatePassphrase
            /// </summary>
            public bool NeedsClientCertificatePassphrase() // TODO: use this
                => !UserDataXml.NeedsLoginCredentials(this)
                && !string.IsNullOrEmpty(ClientCertificate)
                && !CertificateIsValid;

            /// <summary>
            /// Reads and adds the user certificate to be installed along with the wlan profile
            /// </summary>
            /// <param name="filePath">path to the certificate file in question. PKCS12</param>
            /// <param name="passphrase">the passphrase to the certificate file in question</param>
            /// <returns>true if valid and installed</returns>
            public bool AddClientCertificate(string filePath, string passphrase = null)
            {
                var valid = VerifyCertificateBundle(filePath, passphrase);

                if (valid)
                {
                    ClientCertificate = Convert.ToBase64String(File.ReadAllBytes(filePath));
                    ClientCertificatePassphrase = passphrase;
                }

                return valid;
            }

            /// <summary>
            /// Sets the passphrase to use when derypting the certificate bundle.
            /// Will only be stored if valid.
            /// </summary>
            /// <param name="passphrase">the passphrase to the certificate</param>
            /// <returns>true if the passphrase was valid and has been stored</returns>
            public bool AddClientCertificatePassphrase(string passphrase)
            {
                var valid = VerifyCertificateBundle(ClientCertificateRaw, passphrase);
                if (valid)
                    ClientCertificatePassphrase = passphrase;
                return valid;
            }

            /// <summary>
            /// Helper function which verifies if the
            /// certificate data and the passphrase is as valid combo
            /// </summary>
            /// <param name="rawCertificateData">Certificate data, PKCS12</param>
            /// <param name="passphrase">the passphrase to the certificate file in question</param>
            /// <returns>true if valid</returns>
            public static bool VerifyCertificateBundle(byte[] rawCertificateData, string passphrase = null)
            {
                try
                {
                    using var testCertificate = new X509Certificate2(rawCertificateData, passphrase);
                }
                catch (CryptographicException ex)
                {
                    if ((ex.HResult & 0xFFFF) == 0x56) return false; // wrong passphrase
                    throw;
                }
                return true;
            }

            /// <summary>
            /// Helper function which verifies if the filepath exists and
            /// that the passphrase is valid to read the certificate bundle
            /// </summary>
            /// <param name="filePath">path to the certificate file in question. PKCS12</param>
            /// <param name="passphrase">the passphrase to the certificate file in question</param>
            /// <returns>true if valid</returns>
            public static bool VerifyCertificateBundle(string filePath, string passphrase = null)
            {
                if (filePath == null)
                    throw new ArgumentNullException(paramName: nameof(filePath));
                if (!File.Exists(filePath))
                    throw new ArgumentException(paramName: nameof(filePath), message: "file not found");

                return VerifyCertificateBundle(File.ReadAllBytes(filePath), passphrase);
            }

            // Constructor
            public AuthenticationMethod(
                EapType eapType,
                InnerAuthType innerAuthType,
                List<string> serverCertificateAuthorities,
                List<string> serverName,
                string clientUserName = null,
                string clientPassword = null,
                string clientCertificate = null,
                string clientCertificatePassphrase = null,
                string clientOuterIdentity = null,
                string innerIdentitySuffix = null,
                bool innerIdentityHint = false
            )
            {
                EapType = eapType;
                InnerAuthType = innerAuthType;
                ServerCertificateAuthorities = serverCertificateAuthorities ?? new List<string>();
                ServerNames = serverName ?? new List<string>();
                ClientUserName = clientUserName;
                ClientPassword = clientPassword;
                ClientCertificate = clientCertificate;
                ClientCertificatePassphrase = clientCertificatePassphrase;
                ClientOuterIdentity = clientOuterIdentity;
                ClientInnerIdentitySuffix = innerIdentitySuffix;
                ClientInnerIdentityHint = innerIdentityHint;
            }
        }

        /// <summary>
        /// ProviderInfo contains information about the config file's provider.
        /// </summary>
        public readonly struct ProviderInfo
        {
            // Properties
            public string DisplayName { get; }
            public string Description { get; }
            public byte[] LogoData { get; }
            public string LogoMimeType { get; }
            public string EmailAddress { get; }
            public string WebAddress { get; }
            public string Phone { get; }
            public string InstId { get; }
            public string TermsOfUse { get; }
            public (double Latitude, double Longitude)? Location { get; } // nullable coordinates on the form (Latitude, Longitude)

            // Constructor
            public ProviderInfo(
                string displayName,
                string description,
                byte[] logoData,
                string logoMimeType,
                string emailAddress,
                string webAddress,
                string phone,
                string instId,
                string termsOfUse,
                ValueTuple<double, double>? location)
            {
                DisplayName = displayName;
                Description = description;
                LogoData = logoData;
                LogoMimeType = logoMimeType;
                EmailAddress = emailAddress;
                WebAddress = webAddress;
                Phone = phone;
                InstId = instId;
                TermsOfUse = termsOfUse;
                Location = location;
            }
        }

        /// <summary>
        /// Container for an entry in the 'CredentialApplicabilitis' section in the EAP config xml.
        /// Each entry denotes a way to configure this EAP config.
        /// There are threedifferent cases:
        /// - WPA with SSID:
        ///         NetworkType == IEEE80211 and Ssid != null
        /// - WPA with Hotspot2.0:
        ///         NetworkType == IEEE80211 and ConsortiumOid != null
        /// - Wired 801x: 
        ///         NetworkType == IEEE80211 and NetworkId != null
        /// </summary>
        public readonly struct CredentialApplicability
        {
            public IEEE802x NetworkType { get; }

            // IEEE80211 only:

            /// <summary>
            /// NetworkType == IEEE80211 only. Used to configure WPA with SSID
            /// </summary>
            public string Ssid { get; } // Wifi SSID, TODO: use
            /// <summary>
            /// NetworkType == IEEE80211 only. Used to configure WPA with Hotspot 2.0
            /// </summary>
            public string ConsortiumOid { get; } // Hotspot2.0
            /// <summary>
            /// NetworkType == IEEE80211 only, Has either a value of "TKIP" or "CCMP"
            /// </summary>
            public string MinRsnProto { get; } // "TKIP" or "CCMP"


            // IEEE8023 only:

            /// <summary>
            /// NetworkType == IEEE8023 only
            /// </summary>
            public string NetworkId { get; }

            private CredentialApplicability(
                IEEE802x networkType,
                string ssid,
                string consortiumOid,
                string minRsnProto,
                string networkId)
            {

                NetworkType = networkType;
                Ssid = ssid;
                ConsortiumOid = consortiumOid;
                MinRsnProto = minRsnProto;
                NetworkId = networkId;
            }

            public static CredentialApplicability IEEE80211(
                string ssid,
                string consortiumOid,
                string minRsnProto)
            {
                return new CredentialApplicability(
                    IEEE802x.IEEE80211,
                    ssid,
                    consortiumOid,
                    minRsnProto ?? "CCMP",
                    null);
            }

            public static CredentialApplicability IEEE8023(
                string networkId)
            {
                return new CredentialApplicability(
                    IEEE802x.IEEE8023,
                    null,
                    null,
                    null,
                    networkId);
            }

        }


        /// <summary>
        /// Creates a new EapConfig object from EAP config xml data
        /// </summary>
        /// <param name="eapConfigXmlData">EAP config XML as string</param>
        /// <returns>EapConfig object</returns>
        public static EapConfig FromXmlData(string uid, string eapConfigXmlData)
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
            catch (XmlException ex)
            {
                throw new EduroamAppUserError("xml parse exception",
                    "The institution or profile is either not supported or malformed. " +
                    "Please select a different institution or profile.\n\n" +
                    "Exception: " + ex.Message);
            }
            /*
            foreach (XElement eapIdentityProvider in eapConfigXml.Descendants().Where(nameIs("EAPIdentityProvider")))
            {
                // NICE TO HAVE: yield return from this
            }
            */

            // create a new empty list for authentication methods
            List<EapConfig.AuthenticationMethod> authMethods =
                new List<EapConfig.AuthenticationMethod>();

            // iterate over all AuthenticationMethods elements from xml
            foreach (XElement authMethodXml in eapConfigXml.Descendants().Where(nameIs("AuthenticationMethod")))
            {
                XElement serverSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ServerSideCredential"));
                XElement clientSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ClientSideCredential"));

                // get EAP method type
                EapType eapType = (EapType)(int)authMethodXml
                    .Elements().First(nameIs("EAPMethod"))
                    .Elements().First(nameIs("Type"));

                InnerAuthType innerAuthType = (InnerAuthType?)(int?)authMethodXml
                    .Elements().FirstOrDefault(nameIs("InnerAuthenticationMethod"))
                    ?.Descendants().FirstOrDefault(nameIs("Type"))
                    ?? InnerAuthType.None;

                // ServerSideCredential

                // get list of strings of CA certificates
                List<string> serverCAs = serverSideCredentialXml
                    .Elements().Where(nameIs("CA")) // TODO: <CA format="X.509" encoding="base64"> is assumed, schema does not enforce this
                    .Select(xElement => (string)xElement)
                    .ToList();

                // get list of strings of server IDs
                List<string> serverNames = serverSideCredentialXml
                    .Elements().Where(nameIs("ServerID"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // ClientSideCredential

                // Preset credentials
                var clientUserName = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("UserName"));
                var clientPassword = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Password"));
                var clientCert = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("ClientCertificate")); // TODO: <ClientCertificate format="PKCS12" encoding="base64"> is assumed
                var clientCertPasswd = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Passphrase"));

                // inner/outer identity
                var clientOuterIdentity = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("OuterIdentity"));
                var clientInnerIdentitySuffix = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
                var clientInnerIdentityHint = (bool?)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentityHint")) ?? false;

                // create new authentication method object and adds it to list
                authMethods.Add(new EapConfig.AuthenticationMethod(
                    eapType,
                    innerAuthType,
                    serverCAs,
                    serverNames,
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
            List<EapConfig.CredentialApplicability> credentialApplicabilities =
                new List<EapConfig.CredentialApplicability>();

            foreach (XElement credentialApplicabilityXml in eapConfigXml.Descendants().First(nameIs("CredentialApplicability")).Elements())
            {
                credentialApplicabilities.Add(credentialApplicabilityXml.Name.LocalName switch
                {
                    "IEEE80211" =>
                        CredentialApplicability.IEEE80211(
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("SSID")),
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("ConsortiumOID")),
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("MinRSNProto"))
                        ),
                    "IEEE8023" =>
                        CredentialApplicability.IEEE8023(
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("NetworkID"))
                        ),
                    _ => throw new NotImplementedException(),
                });
            }

            // get logo and identity element
            XElement logoElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderLogo"));
            XElement eapIdentityElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("EAPIdentityProvider")); // NICE TO HAVE: update this if the yield return above gets used

            // get institution ID from identity element
            var instId = (string)eapIdentityElement.Attribute("ID");

            // get provider's logo as base64 encoded string and its mime-type
            var logoData = Convert.FromBase64String((string)logoElement ?? "");
            var logoMimeType = (string)logoElement?.Attribute("mime");

            // Read ProviderInfo attributes:
            XElement providerInfoXml = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderInfo"));

            var displayName = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("DisplayName"));
            var description = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Description"));
            var emailAddress = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("EmailAddress"));
            var webAddress = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("WebAddress"));
            var phone = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("Phone"));
            var termsOfUse = (string)providerInfoXml
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
                uid,
                authMethods,
                credentialApplicabilities,
                new EapConfig.ProviderInfo(
                    displayName ?? string.Empty,
                    description ?? string.Empty,
                    logoData,
                    logoMimeType ?? string.Empty,
                    emailAddress ?? string.Empty,
                    webAddress ?? string.Empty,
                    phone ?? string.Empty,
                    instId ?? string.Empty,
                    termsOfUse ?? string.Empty,
                    location)
            );
        }

        /// <summary>
        /// If this returns true, then the user must provide the login credentials
        /// when installing with ConnectToEduroam or EduroamNetwork
        /// </summary>
        public bool NeedsLoginCredentials()
            => AuthenticationMethods
                .Any(authMethod => authMethod.NeedsLoginCredentials());

        /// <summary>
        /// If this is true, then you must provide a
        /// certificate file and add it with this.AddClientCertificate
        /// </summary>
        public bool NeedsClientCertificate()
            => AuthenticationMethods
                .Any(authMethod => authMethod.NeedsClientCertificate());

        /// <summary>
        /// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
        /// Add this passphrase with this.AddClientCertificatePassphrase
        /// </summary>
        public bool NeedsClientCertificatePassphrase()
            => AuthenticationMethods
                .Any(authMethod => authMethod.NeedsClientCertificatePassphrase());

        /// <summary>
        /// Reads and adds the user certificate to be installed along with the wlan profile
        /// </summary>
        /// <param name="filePath">path to the certificate file in question. PKCS12</param>
        /// <param name="passphrase">the passphrase to the certificate file in question</param>
        /// <returns>true if valid and installed</returns>
        public bool AddClientCertificate(string certificatePath, string certificatePassphrase = null)
            => AuthenticationMethods
                .Where(authMethod => authMethod
                    .AddClientCertificate(certificatePath, certificatePassphrase))
                .ToList().Any(); // evaluate all

        /// <summary>
        /// Sets the passphrase to use when derypting the certificate bundle.
        /// Will only be stored if valid.
        /// </summary>
        /// <param name="passphrase">the passphrase to the certificate</param>
        /// <returns>true if the passphrase was valid and has been stored</returns>
        public bool AddClientCertificatePassphrase(string certificatePassphrase)
            => AuthenticationMethods
                .Where(authMethod => authMethod
                    .AddClientCertificatePassphrase(certificatePassphrase))
                .ToList().Any(); // evaluate all

        /// <summary>
        /// goes through all the AuthentificationMethods in this config and tries to reason about a correct method
        /// </summary>
        /// <returns>A ValueTuple with the inner identity suffix and hint</returns>
        public (string suffix, bool hint) GetClientInnerIdentityRestrictions()
        {
            var hint = AuthenticationMethods
                .All(authMethod => authMethod.ClientInnerIdentityHint);
            var suffi = AuthenticationMethods
                .Select(authMethod => authMethod.ClientInnerIdentitySuffix)
                .ToList();

            string suffix = null;
            if (suffi.Any())
            {
                var first = suffi.First();
                if (suffi.All(suffix => suffix == first))
                    suffix = first;
            }
            return (suffix, hint);
        }

    }

    public enum IEEE802x
    {
        /// <summary>
        /// Wired LAN
        /// </summary>
        IEEE8023, // TODO: add full support for this

        /// <summary>
        /// Wireless LAN
        /// </summary>
        IEEE80211
    }

    /// <summary>
    ///  https://www.vocal.com/secure-communication/eap-types/
    /// </summary>
    public enum EapType
    {
        TLS = 13,
        TTLS = 21,
        PEAP = 25,
        MSCHAPv2 = 26,
    }

    /// <summary>
    /// The type of authentification used in the inner tunnel.
    /// Also known as stage 2 authentification.
    /// </summary>
    public enum InnerAuthType
    {
        // For those EAP types with no inner auth method (TLS and MSCHAPv2)
        None = 0,
        // Non-EAP methods
        PAP = 1,
        //CHAP = NaN, // Not defined in EapConfig schema
        MSCHAP = 2,
        MSCHAPv2 = 3,
        // Tunneled Eap methods
        EAP_PEAP_MSCHAPv2 = 25,
        EAP_MSCHAPv2 = 26,
    }

}
