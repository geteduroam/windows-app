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
			public string Logo { get; set; } = null;
			public string EmailAddress { get; set; }
			public string WebAddress { get; set; }
			public string Phone { get; set; }
			public string InstId { get; set; }

			public ProviderInfo(string displayName, string logo, string emailAddress, string webAddress, string phone, string instId)
			{
				DisplayName = displayName;
				Logo = logo;
				EmailAddress = emailAddress;
				WebAddress = webAddress;
				Phone = phone;
				InstId = instId;
			}
		}

	}
}
