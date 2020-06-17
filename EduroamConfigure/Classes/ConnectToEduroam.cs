using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedNativeWifi;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace EduroamConfigure
{
	/// <summary>
	/// Contains various functions for:
	/// - installing certificates
	/// - creating a wireless profile
	/// - setting user data
	/// - connecting to a network
	/// </summary>
	class ConnectToEduroam
	{
		// SSID of eduroam network
		private static string Ssid { get; set; }
		// Id of wireless network interface
		private static Guid InterfaceId { get; set; }
		// xml file for building wireless profile
		private static string ProfileXml { get; set; }
		// EAP type of selected configuration
		private static uint EapType { get; set; }
		// client certificate valid from
		public static DateTime CertValidFrom { get; set; }

		/// <summary>
		/// Creates EapConfig object from EAP config file.
		/// </summary>
		/// <param name="eapFile">EAP config file as string.</param>
		/// <returns>EapConfig object.</returns>
		public static EapConfig GetEapConfig(string eapFile)
		{
			// loads the XML file from its file path
			XElement doc = XElement.Parse(eapFile);

			// creates new EapConfig object
			var eapConfig = new EapConfig();
			// creates new list of authentication methods
			List<EapConfig.AuthenticationMethod> authMethods = new List<EapConfig.AuthenticationMethod>();

			// gets all AuthenticationMethods elements from xml
			IEnumerable<XElement> authMethodElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "AuthenticationMethod");
			foreach (XElement element in authMethodElements)
			{
				// gets EAP method type
				var eapTypeEl = (uint)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Type");

				// gets list of CAs
				List<XElement> caElements = element.DescendantsAndSelf().Elements().Where(x => x.Name.LocalName == "CA").ToList();

				// gets string value of CAs and puts them in new list
				List<string> certAuths = new List<string>();
				foreach (XElement caElement in caElements)
				{
					certAuths.Add((string)caElement);
				}

				// gets list of server names
				List<XElement> serverElements = element.DescendantsAndSelf().Elements().Where(x => x.Name.LocalName == "ServerID").ToList();

				// gets string value of server elements and puts them in new list
				List<string> serverNames = new List<string>();
				foreach (XElement serverElement in serverElements)
				{
					serverNames.Add((string)serverElement);
				}

				// gets client certificate
				var clientCert = (string)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "ClientCertificate");

				// gets client cert passphrase
				var passphrase = (string)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Passphrase");

				// creates new authentication method object and adds it to list
				authMethods.Add(new EapConfig.AuthenticationMethod(eapTypeEl, certAuths, serverNames, clientCert, passphrase));
			}
			// adds the authentication method objects to the EapConfig object
			eapConfig.AuthenticationMethods = authMethods;

			// gets provider's  display name
			var displayName = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "DisplayName");
			// gets logo element
			XElement logoElement = doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "ProviderLogo");
			// gets provider's logo as base64 encoded string from logo element
			var logo = Convert.FromBase64String((string)logoElement ?? "");
			// gets the file format of the logo
			var logoFormat = (string)logoElement?.Attribute("mime");
			// gets provider's email address
			var emailAddress = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "EmailAddress");
			// gets provider's web address
			var webAddress = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "WebAddress");
			// gets provider's phone number
			var phone = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Phone");
			// gets terms of use
			var termsOfUse = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "TermsOfUse");
			// gets identity element
			XElement eapIdentityElement = doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "EAPIdentityProvider");
			// gets institution ID from identity element
			var instId = (string)eapIdentityElement?.Attribute("ID");

			// adds the provider info to the EapConfig object
			eapConfig.InstitutionInfo = new EapConfig.ProviderInfo(displayName ?? string.Empty, logo, logoFormat ?? string.Empty, emailAddress ?? string.Empty, webAddress ?? string.Empty, phone ?? string.Empty, instId ?? string.Empty, termsOfUse ?? string.Empty);

			// returns the EapConfig object
			return eapConfig;
		}

		/// <summary>
		/// Installs certificates and creates a wireless profile using an EapConfig object.
		/// </summary>
		/// <param name="eapConfig">EapConfig object.</param>
		/// <returns>Eap type (13, 25, etc.)</returns>
		public static uint Setup(EapConfig eapConfig)
		{
			// creates new instance of eduroam network
			var eduroamInstance = new EduroamNetwork();
			// gets SSID
			Ssid = eduroamInstance.Ssid;
			// gets interface ID
			InterfaceId = eduroamInstance.InterfaceId;

			// gets the first/default authentication method of EapConfig object
			EapConfig.AuthenticationMethod firstAuthMethod = eapConfig.AuthenticationMethods.First();

			// gets EAP type of authentication method
			uint firstEapType = firstAuthMethod.EapType;
			uint eapType = 0;

			foreach (EapConfig.AuthenticationMethod authMethod in eapConfig.AuthenticationMethods)
			{
				// if EAP type is not supported, cancel setup
				if (authMethod.EapType != 13 && authMethod.EapType != 25 && authMethod.EapType != 21) continue;
				// We do not support TTLS yet
				if (authMethod.EapType == 21)
				{
					// Since this profile supports TTLS, be sure that any error returned is about TTLS not being supported
					firstEapType = 21;
					continue;
				}
				eapType = SetupAuthentication(authMethod);
				if (eapType > 0) return EapType = eapType;
			}
			return EapType = firstEapType;
		}

		private static uint SetupAuthentication(EapConfig.AuthenticationMethod authMethod)
		{
			// name of client certificate issuer
			string certIssuer = null;

			// checks if Athentication method contains a client certificate
			if (!string.IsNullOrEmpty(authMethod.ClientCertificate))
			{
				// gets passphrase element
				string clientPwd = authMethod.ClientPassphrase;
				// converts from base64
				var clientBytes = Convert.FromBase64String(authMethod.ClientCertificate);
				// creates certificate object
				var clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
				// sets friendly name of certificate
				clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);

				// opens the personal certificate store
				var personalStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				personalStore.Open(OpenFlags.ReadWrite);

				// adds client cert to personal store
				personalStore.Add(clientCert);

				// closes personal store
				personalStore.Close();

				// gets name of CA that issued the certificate
				certIssuer = clientCert.IssuerName.Name;
				// gets valid from time of certificate
				CertValidFrom = clientCert.NotBefore;
			}

			// opens the trusted root CA store
			var rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			rootStore.Open(OpenFlags.ReadWrite);

			// all CA thumbprints that will be added to Wireless Profile XML
			List<string> thumbprints = new List<string>();

			// gets all CAs from Authentication method
			foreach (string ca in authMethod.CertificateAuthorities)
			{
				// converts from base64
				var caBytes = Convert.FromBase64String(ca);

				// creates certificate object
				var caCert = new X509Certificate2(caBytes);
				// sets friendly name of CA
				caCert.FriendlyName = caCert.GetNameInfo(X509NameType.SimpleName, false);

				// show messagebox to let users know about the CA installation warning if CA not already installed
				X509Certificate2Collection certExists = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, true);
				if (certExists.Count < 1)
				{
					//MessageBox.Show("You will now be prompted to install a Certificate Authority. \n" +
					//                "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
					//                "Accept Certificate Authority", MessageBoxButtons.OK);
					//
					// if CA not installed succesfully, ask user to retry
					var addCaSuccess = false;
					while (!addCaSuccess)
					{
						try
						{
							// adds CA to trusted root store
							rootStore.Add(caCert);
							// if CA added succesfully, stop looping
							addCaSuccess = true;
						}
						catch (CryptographicException ex)
						{
							// if user selects No when prompted to install CA, show messagebox and ask to retry or cancel
							if ((uint)ex.HResult == 0x800704C7)
							{
								//DialogResult retryCa = MessageBox.Show("CA not installed. \nIn order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
								//                                       "Accept Certificate Authority", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								// if user selects cancel, stop looping
								//if (retryCa == DialogResult.Cancel)
								//{
								//    return 0;
								//}
							}
							// if different error message, stop looping
							else
							{
								throw;
							}
						}
					}
				}
				// gets CA thumbprint and formats it
				string formattedThumbprint = Regex.Replace(caCert.Thumbprint, ".{2}", "$0 ");
				// adds thumbprint to list
				thumbprints.Add(formattedThumbprint);
			}

			// gets thumbprints of already installed CAs that match client certificate issuer
			if (certIssuer != null)
			{
				// gets CAs by client certificate issuer name
				X509Certificate2Collection existingCa = rootStore.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, certIssuer, true);

				foreach (X509Certificate2 ca in existingCa)
				{
					// gets CA thumbprint and formats it
					string formattedThumbprint = Regex.Replace(ca.Thumbprint, ".{2}", "$0 ");
					// adds thumbprint to list
					thumbprints.Add(formattedThumbprint);
				}
			}

			// closes trusted root store
			rootStore.Close();

			// gets server names of authentication method and joins them into one single string
			string serverNames = string.Join(";", authMethod.ServerName);

			// generates new profile xml
			ProfileXml = EduroamApp.ProfileXml.CreateProfileXml(Ssid, authMethod.EapType, serverNames, thumbprints);

			// creates a new wireless profile
			CreateNewProfile();

			// checks if EAP type is TLS and there is no client certificate
			if (authMethod.EapType == 13 && string.IsNullOrEmpty(authMethod.ClientCertificate))
			{
				// prompts the user for a locally stored client certificate file
				DialogResult dialogResult = MessageBox.Show(
					"The selected profile requires a separate client certificate. Do you want to browse your local files for one?",
					"Client certificate required", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
				return (uint)(dialogResult == DialogResult.Yes ? 500 : 0);
			}

			// returns EAP type of installed authentication method
			return authMethod.EapType;
		}

		/// <summary>
		/// Creates new network profile according to selected network and profile XML.
		/// </summary>
		/// <returns>True if profile create success, false if not.</returns>
		public static bool CreateNewProfile()
		{
			// sets the profile type to be All-user (value = 0)
			const ProfileType profileType = ProfileType.AllUser;

			// security type not required
			const string securityType = null;

			// overwrites if profile already exists
			const bool overwrite = true;

			return NativeWifi.SetProfile(InterfaceId, profileType, ProfileXml, securityType, overwrite);
		}

		/// <summary>
		/// Deletes eduroam profile.
		/// </summary>
		/// <returns>True if profile delete succesful, false if not.</returns>
		public static bool RemoveProfile()
		{
			return NativeWifi.DeleteProfile(InterfaceId, Ssid);
		}

		/// <summary>
		/// Creates user data xml for connecting using credentials.
		/// </summary>
		/// <param name="username">User's username.</param>
		/// <param name="password">User's password.</param>
		public static void SetupLogin(string username, string password)
		{
			// generates user data xml file
			string userDataXml = UserDataXml.CreateUserDataXml(username, password, EapType);
			// sets user data
			SetUserData(InterfaceId, Ssid, userDataXml);
		}

		/// <summary>
		/// Sets user data for a wireless profile.
		/// </summary>
		/// <param name="networkId">Interface ID of selected network.</param>
		/// <param name="profileName">Name of associated wireless profile.</param>
		/// <param name="userDataXml">User data XML converted to string.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public static bool SetUserData(Guid networkId, string profileName, string userDataXml)
		{
			// sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"
			const uint profileUserType = 0x00000001;

			return NativeWifi.SetProfileUserData(networkId, profileName, profileUserType, userDataXml);
		}

		/// <summary>
		/// Waits for async connection to complete.
		/// </summary>
		/// <returns>Connection result.</returns>
		public static Task<bool> WaitForConnect()
		{
			// runs async method
			Task<bool> connectResult = Task.Run(ConnectAsync);
			return connectResult;
		}

		/// <summary>
		/// Connects to the chosen wireless LAN.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		private static async Task<bool> ConnectAsync()
		{
			// gets updated eduroam network pack
			AvailableNetworkPack network = EduroamNetwork.GetEduroamPack();

			if (network == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: network.Interface.Id,
				profileName: network.ProfileName,
				bssType: network.BssType,
				timeout: TimeSpan.FromSeconds(5));
		}
	}
}
