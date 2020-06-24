using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace EduroamApp
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
            public AuthenticationMethod(EapType eapType, List<string> certificateAuthorities, List<string> serverName, string clientCertificate = null, string clientPassphrase = null)
            {
                EapType = eapType;
                CertificateAuthorities = certificateAuthorities;
                ServerName = serverName;
                ClientCertificate = clientCertificate;
                ClientPassphrase = clientPassphrase;
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
            public ProviderInfo(string displayName, byte[] logo, string logoFormat, string emailAddress, string webAddress, string phone, string instId, string termsOfUse)
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
            // TODO: Hotspot 2.0
            // TODO: TTLS

            // load the XML file from its file path
            XElement doc = XElement.Parse(eapXmlData);
            IEnumerable<XElement> docElements() => doc.DescendantsAndSelf().Elements(); // shorthand lambda

            // create new list of authentication methods
            List<EapConfig.AuthenticationMethod> authMethods = new List<EapConfig.AuthenticationMethod>();

            // get all AuthenticationMethods elements from xml
            IEnumerable<XElement> authMethodElements = docElements().Where(cl => cl.Name.LocalName == "AuthenticationMethod");
            foreach (XElement element in authMethodElements)
            {
                IEnumerable<XElement> elementElements() => element.DescendantsAndSelf().Elements(); // shorthand lambda

                // get EAP method type
                var eapTypeEl = (EapType)(uint)elementElements().FirstOrDefault(x => x.Name.LocalName == "Type");

                // get string value of CAs
                IEnumerable<XElement> caElements = elementElements().Where(x => x.Name.LocalName == "CA");
                List<string> certAuths = caElements.Select((caElement) => (string)caElement).ToList();

                // get string value of server elements
                IEnumerable<XElement> serverElements = elementElements().Where(x => x.Name.LocalName == "ServerID");
                List<string> serverNames = serverElements.Select((serverElement) => (string)serverElement).ToList();

                // get client certificate
                var clientCert = (string)elementElements().FirstOrDefault(x => x.Name.LocalName == "ClientCertificate");

                // get client cert passphrase
                var passphrase = (string)elementElements().FirstOrDefault(x => x.Name.LocalName == "Passphrase");

                // create new authentication method object and adds it to list
                authMethods.Add(new EapConfig.AuthenticationMethod(eapTypeEl, certAuths, serverNames, clientCert, passphrase));
            }


            // get logo and identity element
            XElement logoElement = docElements().FirstOrDefault(x => x.Name.LocalName == "ProviderLogo");
            XElement eapIdentityElement = docElements().FirstOrDefault(x => x.Name.LocalName == "EAPIdentityProvider");

            // get provider's  display name
            var displayName = (string)docElements().FirstOrDefault(x => x.Name.LocalName == "DisplayName");
            // get provider's logo as base64 encoded string from logo element
            var logo = Convert.FromBase64String((string)logoElement ?? "");
            // get the file format of the logo
            var logoFormat = (string)logoElement?.Attribute("mime");
            // get provider's email address
            var emailAddress = (string)docElements().FirstOrDefault(x => x.Name.LocalName == "EmailAddress");
            // get provider's web address
            var webAddress = (string)docElements().FirstOrDefault(x => x.Name.LocalName == "WebAddress");
            // get provider's phone number
            var phone = (string)docElements().FirstOrDefault(x => x.Name.LocalName == "Phone");
            // get institution ID from identity element
            var instId = (string)eapIdentityElement?.Attribute("ID");
            // get terms of use
            var termsOfUse = (string)docElements().FirstOrDefault(x => x.Name.LocalName == "TermsOfUse");

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
