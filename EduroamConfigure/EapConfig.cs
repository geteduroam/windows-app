using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Stores information found in an EAP-config file.
    /// </summary>
    public class EapConfig
    {
        // Properties
        public List<AuthenticationMethod> AuthenticationMethods { get; }
        public ProviderInfo InstitutionInfo { get; }

        // Constructor
        EapConfig(List<AuthenticationMethod> authenticationMethods, ProviderInfo institutionInfo)
        {
            AuthenticationMethods = authenticationMethods;
            InstitutionInfo = institutionInfo;
        }

        
        /// <summary>
        /// AuthenticationMethod contains information about client certificates and CAs.
        /// </summary>
        public class AuthenticationMethod
        {
            // Properties
            public EapType EapType { get; }
            public InnerAuthType InnerAuthType { get; }
            public List<string> CertificateAuthorities { get; } // TODO: document format, probably DER in base64?
            public List<string> ServerNames { get; }
            public string ClientCertificate { get; } // TODO: document format, probably PKCS12 in base64?
            public string ClientCertificatePassphrase { get; }
            public string InnerIdentitySuffix { get; }
            public bool InnerIdentityHint { get; }

            /// <summary>
            /// Enumerates CertificateAuthorities as X509Certificate2 objects
            /// </summary>
            public IEnumerable<X509Certificate2> CertificateAuthoritiesAsX509Certificate2() {
                foreach (var ca in CertificateAuthorities)
                {
                    var cert = new X509Certificate2(Convert.FromBase64String(ca));
                    cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);
                    yield return cert;
                }
            }

            // Constructor
            public AuthenticationMethod(
                EapType eapType,
                InnerAuthType innerAuthType,
                List<string> certificateAuthorities,
                List<string> serverName,
                string clientCertificate = null,
                string clientCertificatePassphrase = null,
                string innerIdentitySuffix = null,
                bool innerIdentityHint = false
            )
            {
                EapType = eapType;
                InnerAuthType = innerAuthType;
                CertificateAuthorities = certificateAuthorities;
                ServerNames = serverName;
                ClientCertificate = clientCertificate;
                ClientCertificatePassphrase = clientCertificatePassphrase;
                InnerIdentitySuffix = innerIdentitySuffix;
                InnerIdentityHint = innerIdentityHint;
            }
        }

        /// <summary>
        /// ProviderInfo contains information about the config file's provider.
        /// </summary>
        public class ProviderInfo
        {
            // Properties
            public string DisplayName { get; }
            public byte[] LogoData { get; }
            public string LogoMimeType { get; }
            public string EmailAddress { get; }
            public string WebAddress { get; }
            public string Phone { get;  }
            public string InstId { get; }
            public string TermsOfUse { get; }

            // Constructor
            public ProviderInfo(
                string displayName,
                byte[] logoData,
                string logoMimeType,
                string emailAddress,
                string webAddress,
                string phone,
                string instId,
                string termsOfUse)
            {
                DisplayName = displayName;
                LogoData = logoData;
                LogoMimeType = logoMimeType;
                EmailAddress = emailAddress;
                WebAddress = webAddress;
                Phone = phone;
                InstId = instId;
                TermsOfUse = termsOfUse.Replace("\r\n", "");
            }
        }


        /// <summary>
        /// Creates a new EapConfig object from EAP config xml data
        /// </summary>
        /// <param name="eapConfigXmlData">EAP config XML as string</param>
        /// <returns>EapConfig object</returns>
        public static EapConfig FromXmlData(string eapConfigXmlData)
        {
            // XML format Documentation:
            // Current:  https://github.com/GEANT/CAT/blob/master/devices/eap_config/eap-metadata.xsd
            // Outdated: https://tools.ietf.org/id/draft-winter-opsawg-eap-metadata-00.html

            // TODO: Hotspot 2.0
            // TODO: TTLS

            static Func<XElement, bool> nameIs(string name) => // shorthand lambda
                element => element.Name.LocalName == name;

            // load the XML file into a XElement object
            XElement eapConfigXml = XElement.Parse(eapConfigXmlData);
            foreach (XElement eapIdentityProvider in eapConfigXml.Descendants().Where(nameIs("EAPIdentityProvider")))
            {
                //TODO: yield return from this
            }

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
                EapType eapType = (EapType)(uint)authMethodXml
                    .Elements().First(nameIs("EAPMethod"))
                    .Elements().First(nameIs("Type"));

                InnerAuthType innerAuthType = (InnerAuthType)(uint)authMethodXml
                    .Elements().FirstOrDefault(nameIs("InnerAuthenticationMethod"))
                    ?.Descendants().FirstOrDefault(nameIs("Type"));

                // get list of strings of CA certificates
                List<string> certAuths = serverSideCredentialXml
                    .Elements().Where(nameIs("CA"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // get list of strings of server IDs
                List<string> serverNames = serverSideCredentialXml
                    .Elements().Where(nameIs("ServerID"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // Get user certificate values
                var clientCert = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("ClientCertificate"));
                var clientCertPasswd = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Passphrase"));

                // Get inner identity values
                var innerIdentitySuffix = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
                var innerIdentityHint = "True" == (string)clientSideCredentialXml // TODO: will cast to bool work?
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentityHint"));

                // create new authentication method object and adds it to list
                authMethods.Add(new EapConfig.AuthenticationMethod(
                    eapType,
                    innerAuthType,
                    certAuths,
                    serverNames,
                    clientCert,
                    clientCertPasswd,
                    innerIdentitySuffix,
                    innerIdentityHint
                ));
            }

            // get logo and identity element
            XElement logoElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderLogo"));
            XElement eapIdentityElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("EAPIdentityProvider")); // TODO: remove

            // get institution ID from identity element
            var instId = (string)eapIdentityElement.Attribute("ID"); // TODO: obsolete?

            // get provider's logo as base64 encoded string and its mime-type
            var logoData = Convert.FromBase64String((string)logoElement ?? "");
            var logoMimeType = (string)logoElement?.Attribute("mime");

            // Read ProviderInfo attributes:
            XElement providerInfoXml = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderInfo"));

            var displayName = (string)providerInfoXml
                ?.Descendants().FirstOrDefault(nameIs("DisplayName"));
            var emailAddress = (string)providerInfoXml
                ?.Descendants().FirstOrDefault(nameIs("EmailAddress"));
            var webAddress = (string)providerInfoXml
                ?.Descendants().FirstOrDefault(nameIs("WebAddress"));
            var phone = (string)providerInfoXml
                ?.Descendants().FirstOrDefault(nameIs("Phone"));
            var termsOfUse = (string)providerInfoXml
                ?.Descendants().FirstOrDefault(nameIs("TermsOfUse"));

            // create EapConfig object and adds the info
            return new EapConfig(
                authMethods,
                new EapConfig.ProviderInfo(
                    displayName ?? string.Empty,
                    logoData,
                    logoMimeType ?? string.Empty,
                    emailAddress ?? string.Empty,
                    webAddress ?? string.Empty,
                    phone ?? string.Empty,
                    instId ?? string.Empty,
                    termsOfUse ?? string.Empty)
            );
        }
    }

    /// <summary>
    ///  https://www.vocal.com/secure-communication/eap-types/
    /// </summary>
    public enum EapType : uint
    {
        TLS = 13,
        TTLS = 21,
        PEAP = 25,
    }

    public enum InnerAuthType: uint
    {
        // For those EAP types with no inner auth method
        None = 0,
        // Non-EAP methods
        PAP = 1,
        MSCHAP = 2,
        MSCHAPv2 = 3,
        // Tunneled Eap methods
        EAP_MSCHAPv2 = 26,
    }

}
