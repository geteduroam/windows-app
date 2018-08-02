using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    class AuthenticationMethod
    {
        // Properties
        public uint EapType { get; set; }
        public List<string> CertificateAuthorities { get; set; }
        public string ServerName { get; set; }
        public string ClientCertificate { get; set; }
        public string ClientPassphrase { get; set; }

        // Constructor
        public AuthenticationMethod(uint eapType, List<string> certificateAuthorities, string serverName, string clientCertificate = null, string clientPassphrase = null)
        {
            EapType = eapType;
            CertificateAuthorities = certificateAuthorities;
            ServerName = serverName;
            ClientCertificate = clientCertificate;
            ClientPassphrase = clientPassphrase;
        }

    }
}
