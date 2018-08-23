using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EduroamApp
{
	class ConnectToEduroam
	{
		// sets eduroam as chosen network
		static EduroamNetwork eduroamInstance = new EduroamNetwork(); // creates new instance of eduroam network
		static readonly string ssid = eduroamInstance.Ssid; // gets SSID
		static readonly Guid interfaceId = eduroamInstance.InterfaceId; // gets interface ID

		public static uint Setup(string eapString)
		{
			// gets the first/default authentication method of an EAP config file
			AuthenticationMethod authMethod = GetAuthMethods(eapString).First();

			// gets EAP type of authentication method
			uint eapType = authMethod.EapType;

			// if EAP type is not supported, cancel setup
			if (eapType != 13 && eapType != 25 && eapType != 21) return eapType;

			// opens the trusted root CA store
			X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			rootStore.Open(OpenFlags.ReadWrite);

			// all CA thumbprints that will be added to Wireless Profile XML
			List<string> thumbprints = new List<string>();

			// gets all CAs from Authentication method
			foreach (string ca in authMethod.CertificateAuthorities)
			{
				// converts from base64
				var caBytes = Convert.FromBase64String(ca);

				// creates certificate object
				X509Certificate2 caCert = new X509Certificate2(caBytes);
				// sets friendly name of CA
				caCert.FriendlyName = caCert.GetNameInfo(X509NameType.SimpleName, false);

				// show messagebox to let users know about the CA installation warning if CA not already installed
				var certExists = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, true);
				if (certExists.Count < 1)
				{
					MessageBox.Show("You will now be prompted to install a Certificate Authority. \n" +
									"In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
						"Accept Certificate Authority", MessageBoxButtons.OK);

					// if CA not installed succesfully, ask user to retry
					bool addCaSuccess = false;
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
							// if user selects No when prompted to install CA, show error and ask to retry or cancel
							if ((uint)ex.HResult == 0x800704C7)
							{
								DialogResult retryCa = MessageBox.Show("CA not installed. \nIn order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
																		"Accept Certificate Authority", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								// if user selects cancel, stop looping
								if (retryCa == DialogResult.Cancel)
								{
									addCaSuccess = true;
									return 0;
								}
							}
							else
							// if different error message, stop looping
							{
								addCaSuccess = true;
							}
						}
					}
				}
				// gets CA thumbprint and formats it
				string formattedThumbprint = Regex.Replace(caCert.Thumbprint, ".{2}", "$0 ");
				// adds thumbprint to list
				thumbprints.Add(formattedThumbprint);
			}

			// name of the certificate issuer
			string certIssuer = null;

			// checks if Athentication method contains a client certificate
			if (!string.IsNullOrEmpty(authMethod.ClientCertificate))
			{
				// gets passphrase element
				string clientPwd = authMethod.ClientPassphrase;
				// converts from base64
				var clientBytes = Convert.FromBase64String(authMethod.ClientCertificate);
				// creates certificate object
				X509Certificate2 clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
				// sets friendly name of certificate
				clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);

				// opens the personal certificate store
				X509Store personalStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				personalStore.Open(OpenFlags.ReadWrite);

				// adds client cert to personal store
				personalStore.Add(clientCert);

				// closes personal store
				personalStore.Close();

				// gets the CA that issued the certificate
				certIssuer = clientCert.IssuerName.Name;
			}

			if (certIssuer != null)
			{
				var existingCa = rootStore.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, certIssuer, true);

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
			string profileXml = ProfileXml.CreateProfileXml(ssid, eapType, serverNames, thumbprints);

			// creates a new wireless profile
			Debug.WriteLine(CreateNewProfile(interfaceId, profileXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");

			return eapType;
		}

		public static void SetupLogin(string username, string password, uint eapType)
		{
			// generates user data xml file
			string userDataXml = UserDataXml.CreateUserDataXml(username, password, eapType);
			// sets user data
			SetUserData(interfaceId, ssid, userDataXml);
		}

		public static Task<bool> Connect()
		{
			// gets eduroam network pack
			AvailableNetworkPack network = EduroamNetwork.GetEduroam();

			// connects to eduroam
			Task<bool> connectResult = Task.Run(() => ConnectAsync(network));
			return connectResult;
		}

		/// <summary>
		/// Connects to the chosen wireless LAN.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		private static async Task<bool> ConnectAsync(AvailableNetworkPack chosenWifi)
		{
			if (chosenWifi == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: chosenWifi.Interface.Id,
				profileName: chosenWifi.ProfileName,
				bssType: chosenWifi.BssType,
				timeout: TimeSpan.FromSeconds(6));
		}

		/// <summary>
		/// Creates new network profile according to selected network and profile XML.
		/// </summary>
		/// <param name="networkId">Interface ID of selected network.</param>
		/// <param name="profileXml">Wireless profile XML converted to string.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		private static bool CreateNewProfile(Guid networkId, string profileXml)
		{
			// sets the profile type to be All-user (value = 0)
			// if set to Per User, the security type parameter is not required
			ProfileType newProfileType = ProfileType.AllUser;

			// security type not required
			string newSecurityType = null;

			// overwrites if profile already exists
			bool overwrite = true;

			return NativeWifi.SetProfile(networkId, newProfileType, profileXml, newSecurityType, overwrite);
		}

		/// <summary>
		/// Deletes eduroam profile.
		/// </summary>
		/// <returns>True if delete succesful, false if not.</returns>
		public static bool RemoveProfile()
		{
			return NativeWifi.DeleteProfile(interfaceId, ssid);
		}

		/// <summary>
		/// Sets a profile's user data for login with username + password.
		/// </summary>
		/// <param name="networkId">Interface ID of selected network.</param>
		/// <param name="profileName">Name of associated wireless profile.</param>
		/// <param name="userDataXml">User data XML converted to string.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public static bool SetUserData(Guid networkId, string profileName, string userDataXml)
		{
			// sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"
			uint profileUserType = 0x00000001;

			return NativeWifi.SetProfileUserData(networkId, profileName, profileUserType, userDataXml);
		}

		/// <summary>
		/// Gets all authentication methods from EAP config file.
		/// </summary>
		/// <param name="eapFile">EAP config file as string.</param>
		/// <returns>List of Authentication Method objects</returns>
		public static List<AuthenticationMethod> GetAuthMethods(string eapFile)
		{
			// loads the XML file from its file path
			XElement doc = XElement.Parse(eapFile);

			// gets all AuthenticationMethods elements
			IEnumerable<XElement> authMethodElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "AuthenticationMethod");
			List<AuthenticationMethod> authMethodObjects = new List<AuthenticationMethod>();

			foreach (XElement element in authMethodElements)
			{
				// gets EAP method type
				uint eapType = (uint)element.DescendantsAndSelf().Elements().First(x => x.Name.LocalName == "Type");

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
				string clientCert = (string)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "ClientCertificate");

				// gets client cert passphrase
				string passphrase = (string)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Passphrase");

				// creates new authentication method object and adds it to list
				authMethodObjects.Add(new AuthenticationMethod(eapType, certAuths, serverNames, clientCert, passphrase));
			}

			return authMethodObjects;
		}

		/// <summary>
		/// Gets all client certificates and CAs from EAP config file.
		/// </summary>
		/// <returns>List of client certificates and list of CAs.</returns>
		private static Tuple<List<X509Certificate2>, List<X509Certificate2>> GetCertificates(string fileString)
		{
			// loads the XML file from its file path
			XElement doc = XElement.Parse(fileString);


			// certificate lists to be populated
			List<X509Certificate2> clientCertificates = new List<X509Certificate2>();
			List<X509Certificate2> certAuthorities = new List<X509Certificate2>();

			// gets all ClientSideCredential elements
			IEnumerable<XElement> clientCredElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientSideCredential");

			// gets client certificates and adds them to client certificate list
			foreach (XElement el in clientCredElements)
			{
				// list of ClientCertificate elements
				var certElements = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientCertificate").ToList();
				// checks every ClientSideCredential element for ClientCertificate elements
				if (certElements.Any())
				{
					// there should only be one ClientCertificate in a ClientSideCredential element, so gets the first one
					string base64Client = certElements.First().Value; // Client cert encoded to base64
					if (base64Client != "") // checks that the certificate value is not empty
					{
						// gets passphrase element
						string clientPwd = (string) el.DescendantsAndSelf().Elements().FirstOrDefault(pw => pw.Name.LocalName == "Passphrase"); // Client cert password
						// converts from base64
						var clientBytes = Convert.FromBase64String(base64Client);

						// creates certificate object
						X509Certificate2 clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
						// sets friendly name of certificate
						clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);
						// adds certificate object to list
						clientCertificates.Add(clientCert);
					}
				}
			}

			// gets CAs and adds them to CA list
			IEnumerable<XElement> caElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "CA");
			foreach (XElement ca in caElements)
			{
				// CA encoded to base64
				string base64Ca = ca.Value;
				// checks that the CA value is not empty
				if (base64Ca != "")
				{
					// converts from base64
					var caBytes = Convert.FromBase64String(base64Ca); // CA decoded from base64
					// creates certificate object
					X509Certificate2 caCert = new X509Certificate2(caBytes); // CA object
					// sets friendly name of certificate
					caCert.FriendlyName = caCert.GetNameInfo(X509NameType.SimpleName, false);
					// adds certificate object to list
					certAuthorities.Add(caCert);
				}
			}

			// returns lists of certificates
			return Tuple.Create(clientCertificates, certAuthorities);
		}

		/// <summary>
		/// Installs a certificate object in the specified certificate store.
		/// </summary>
		/// <param name="cert">Certificate object.</param>
		/// <param name="store">Certificate store (personal or root)</param>
		private static void InstallCertificate(X509Certificate2 cert, X509Store store)
		{
			// opens certificate store
			store.Open(OpenFlags.ReadWrite);

			// check if CA already exists in store
			if (store.Name == "Root")
			{
				// show messagebox to let users know about the CA installation warning
				var certExists = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, true);
				if (certExists.Count < 1)
				{
					MessageBox.Show("You will now be prompted to install a Certificate Authority. " +
									"In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
						"Accept Certificate Authority", MessageBoxButtons.OK);
				}
			}

			// adds certificate to store
			store.Add(cert);

			// closes certificate store
			store.Close();
		}

		public static string GetInstId(string eapString)
		{
			// loads XML file from string
			XElement doc = XElement.Parse(eapString);
			// gets institution Id
			string instId = (string)doc.Descendants().ElementAtOrDefault(0).Attribute("ID");

			return instId;
		}
	}
}
