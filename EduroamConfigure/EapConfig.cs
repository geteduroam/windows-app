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
        public List<AuthenticationMethod> AuthenticationMethods { get; set; }
        public ProviderInfo InstitutionInfo { get; set; }


        /// <summary>
        /// AuthenticationMethod contains information about client certificates and CAs.
        /// </summary>
        public class AuthenticationMethod
        {
            // Properties
            public EapType EapType { get; set; }
            public List<string> CertificateAuthorities { get; set; }
            public List<string> ServerName { get; set; }
            public string ClientCertificate { get; set; }
            public string ClientPassphrase { get; set; }
            public string InnerIdentitySuffix { get; set; }
            public bool InnerIdentityHint{ get; set; }

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
                List<string> certificateAuthorities,
                List<string> serverName,
                string clientCertificate = null,
                string clientPassphrase = null,
                string innerIdentitySuffix = null,
                bool InnerIdentityHint = false
            )
            {
                EapType = eapType;
                CertificateAuthorities = certificateAuthorities;
                ServerName = serverName;
                ClientCertificate = clientCertificate;
                ClientPassphrase = clientPassphrase;
                InnerIdentitySuffix = innerIdentitySuffix;
                InnerIdentityHint = InnerIdentityHint;
            }
        }

        /// <summary>
        /// ProviderInfo contains information about the config file's provider.
        /// </summary>
        public class ProviderInfo
        {
            // Properties
            public string DisplayName { get; set; }
            public byte[] Logo { get; set; }
            public string LogoFormat { get; set; }
            public string EmailAddress { get; set; }
            public string WebAddress { get; set; }
            public string Phone { get; set; }
            public string InstId { get; set; }
            public string TermsOfUse { get; set; }

            // Constructor
            public ProviderInfo(
                string displayName, 
                byte[] logo, 
                string logoFormat, 
                string emailAddress, 
                string webAddress, 
                string phone, 
                string instId, 
                string termsOfUse)
            {
                DisplayName = displayName;
                Logo = logo;
                LogoFormat = logoFormat;
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
        /// <param name="eapXmlData">EAP config XML as string</param>
        /// <returns>EapConfig object</returns>
        public static EapConfig FromXmlData(string eapXmlData)
        {
            // XML format Documentation: https://tools.ietf.org/id/draft-winter-opsawg-eap-metadata-00.html

            // TODO: Hotspot 2.0
            // TODO: TTLS

            // load the XML file into a XElement object
            XElement eapXml = XElement.Parse(eapXmlData);
            static Func<XElement, bool> nameIs(string name) => // shorthand lambda
                element => element.Name.LocalName == name;

            // create a new empty list for authentication methods
            List<EapConfig.AuthenticationMethod> authMethods = new List<EapConfig.AuthenticationMethod>();

            // iterate over all AuthenticationMethods elements from xml
            IEnumerable<XElement> authMethodXmlElements = eapXml.Descendants().Where(nameIs("AuthenticationMethod"));
            foreach (XElement authMethodXml in authMethodXmlElements)
            {
                // get EAP method type
                EapType eapType = (EapType)(uint)authMethodXml
                    .Elements().First(nameIs("EAPMethod"))
                    .Elements().First(nameIs("Type"));

                // get EAP method type
                authMethodXml.Elements().Where(nameIs("EapMethod"));

                // get string value of CAs
                IEnumerable<XElement> caElements = authMethodXml.Descendants().Where(nameIs("CA"));
                List<string> certAuths = caElements.Select(caElement => (string)caElement).ToList();

                // get string value of server elements
                IEnumerable<XElement> serverElements = authMethodXml.Descendants().Where(nameIs("ServerID"));
                List<string> serverNames = serverElements.Select((serverElement) => (string)serverElement).ToList();

                // get client certificate
                var clientCert = (string)authMethodXml.Descendants().FirstOrDefault(nameIs("ClientCertificate"));

                // get client cert passphrase
                var clientCertPasswd = (string)authMethodXml.Descendants().FirstOrDefault(nameIs("Passphrase"));

                var InnerIdentitySuffix = (string)authMethodXml
                    .Elements().Where(nameIs("ClientSideCredential"))
                    .Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
                
                var InnerIdentityHint = "True" == (string)authMethodXml
                    .Elements().Where(nameIs("ClientSideCredential"))
                    .Elements().FirstOrDefault(nameIs("InnerIdentityHint"));


                // create new authentication method object and adds it to list
                authMethods.Add(new EapConfig.AuthenticationMethod(
                    eapType, 
                    certAuths, 
                    serverNames, 
                    clientCert,
                    clientCertPasswd,
                    InnerIdentitySuffix,
                    InnerIdentityHint
                ));
            }


            // get logo and identity element
            XElement logoElement = eapXml.Descendants().FirstOrDefault(nameIs("ProviderLogo"));
            XElement eapIdentityElement = eapXml.Descendants().FirstOrDefault(nameIs("EAPIdentityProvider"));

            // get provider's display name
            var displayName = (string)eapXml.Descendants().FirstOrDefault(nameIs("DisplayName"));
            // get provider's logo as base64 encoded string from logo element
            var logo = Convert.FromBase64String((string)logoElement ?? "");
            // get the file format of the logo
            var logoFormat = (string)logoElement?.Attribute("mime");
            // get provider's email address
            var emailAddress = (string)eapXml.Descendants().FirstOrDefault(nameIs("EmailAddress"));
            // get provider's web address
            var webAddress = (string)eapXml.Descendants().FirstOrDefault(nameIs("WebAddress"));
            // get provider's phone number
            var phone = (string)eapXml.Descendants().FirstOrDefault(nameIs("Phone"));
            // get institution ID from identity element
            var instId = (string)eapIdentityElement?.Attribute("ID");
            // get terms of use
            var termsOfUse = (string)eapXml.Descendants().FirstOrDefault(nameIs("TermsOfUse"));

            // create EapConfig object and adds the info
            return new EapConfig
            {
                AuthenticationMethods = authMethods,
                InstitutionInfo = new EapConfig.ProviderInfo(
                    displayName ?? string.Empty,
                    logo,
                    logoFormat ?? string.Empty,
                    emailAddress ?? string.Empty,
                    webAddress ?? string.Empty,
                    phone ?? string.Empty,
                    instId ?? string.Empty,
                    termsOfUse ?? string.Empty)
            };
        }
    }

    /// <summary>
    ///  https://www.vocal.com/secure-communication/eap-types/
    /// </summary>
    public enum EapType : uint
    {
        TLS = 13,
        PEAP = 25, // also covers MSCHAPv2 (26)
        TTLS = 21,
    }

}
