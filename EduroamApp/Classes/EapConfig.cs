using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    public class EapConfig
    {
        public List<AuthenticationMethod> AuthenticationMethods { get; set; }
        public ProviderInfo InstitutionInfo { get; set; }

        public class AuthenticationMethod
        {
            // Properties
            public uint EapType { get; set; }
            public List<string> CertificateAuthorities { get; set; }
            public List<string> ServerName { get; set; }
            public string ClientCertificate { get; set; }
            public string ClientPassphrase { get; set; }

            // Constructor
            public AuthenticationMethod(uint eapType, List<string> certificateAuthorities, List<string> serverName, string clientCertificate = null, string clientPassphrase = null)
            {
                EapType = eapType;
                CertificateAuthorities = certificateAuthorities;
                ServerName = serverName;
                ClientCertificate = clientCertificate;
                ClientPassphrase = clientPassphrase;
            }
        }

        public class ProviderInfo
        {
            public string DisplayName { get; set; }
            public string Logo { get; set; }
            public string LogoFormat { get; set; }
            public string EmailAddress { get; set; }
            public string WebAddress { get; set; }
            public string Phone { get; set; }
            public string InstId { get; set; }
            public string TermsOfUse { get; set; }

            // Constructor
            public ProviderInfo(string displayName, string logo, string logoFormat, string emailAddress, string webAddress, string phone, string instId, string termsOfUse)
            {
                DisplayName = displayName ?? string.Empty; // if value is null, make it ""
                Logo = logo;
                LogoFormat = logoFormat;
                EmailAddress = emailAddress ?? string.Empty; // if value is null, make it ""
                WebAddress = webAddress ?? string.Empty; // if value is null, make it ""
                Phone = phone ?? string.Empty; // if value is null, make it ""
                InstId = instId;
                TermsOfUse = termsOfUse;
            }
        }

    }
}
