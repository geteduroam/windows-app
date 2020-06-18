using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
            /// TODO
            /// </summary>
            /// <returns></returns>
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
    }

    public enum EapType : uint
    {
        TLS = 13,
        PEAP = 25, // also covers MSCHAPv2 (26)
        TTLS = 21,
    }

}
