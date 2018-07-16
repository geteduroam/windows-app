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

namespace EduroamApp
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			InitializeComponent();
		}

		private void frmMain_load(object sender, EventArgs e)
		{
			//// sets eduroam as chosen network
			//AvailableNetworkPack network = SetChosenNetwork();
			//txtOutput.Text = $"Now connecting to {network.Ssid.ToString()}.\n";

			// sets default connection method
			cboMethod.SelectedIndex = 0;
		}


		private void btnTestUserData_Click(object sender, EventArgs e)
		{
			AvailableNetworkPack network = SetChosenNetwork();

			TestProfileUserData(network, "eduroam");
		}

		private void btnSelectProfile_Click(object sender, EventArgs e)
		{
			string filePath = GetXmlFile();
			string fileName = Path.GetFileName(filePath);
			txtProfilePath.Text = fileName;
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			string filePath = GetXmlFile();
			string thumbprint = "8d043f808044894db8d8e06da2acf5f98fb4a610"; // default value is server thumbprint for login with username/password

			// sets eduroam as chosen network
			AvailableNetworkPack network = SetChosenNetwork();
			//TestNewProfile(network, filePath);
			//SetProfileUserData(network, "eduroam");

			string ssid = network.Ssid.ToString();
			Guid interfaceID = network.Interface.Id;

			if (cboMethod.SelectedIndex == 0)
			{
				// downloads and installs client certificate
				string certPassword = txtCertPwd.Text;
				var certificateResult = InstallCertificate(certPassword);
				txtOutput.Text += certificateResult.Item1;
				txtOutput.Text += certificateResult.Item2;
				thumbprint = certificateResult.Item3; // gets thumbprint of CA
			}

			// configures profile xml
			string newXml = ConfigureProfileXml(filePath, ssid, thumbprint);
			//MessageBox.Show(newXml);
			// creates a new network profile

			txtOutput.Text += (CreateNewProfile(interfaceID, newXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");


			if (cboMethod.SelectedIndex == 1)
			{
				string userDataXml = GetXmlFile();
				string username = txtUsername.Text;
				string password = txtPassword.Text;

				string newUserData = ConfigureUserDataXml(userDataXml, username, password);
				MessageBox.Show(newUserData);
				// sets user data
				txtOutput.Text += (SetUserData(interfaceID, ssid, newUserData) ? "User data set successfully.\n" : "Setting user data failed.\n");
			}

			// connects to eduroam
			AvailableNetworkPack updatedNetwork = SetChosenNetwork();
			var connectResult = Task.Run(() => ConnectAsync(updatedNetwork)).Result;
			txtOutput.Text += (connectResult ? "You are now connected to " + updatedNetwork.Ssid.ToString() + ".\n" : "Connection failed.\n");
		}

		// lets user select wether they want to connect to eduroam using a certificate or username and password
		private void cboMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			int selectedMethod = cboMethod.SelectedIndex;

			// shows/hides controls according to chosen connection method
			switch (selectedMethod)
			{
				case 0:
					lblCertPwd.Visible = true;
					txtCertPwd.Visible = true;

					lblUsername.Visible = false;
					lblPassword.Visible = false;
					txtUsername.Visible = false;
					txtPassword.Visible = false;
					break;
				case 1:
					lblUsername.Visible = true;
					lblPassword.Visible = true;
					txtUsername.Visible = true;
					txtPassword.Visible = true;

					lblCertPwd.Visible = false;
					txtCertPwd.Visible = false;
					break;
			}
		}


		// -------------------------------------------- FUNCIONS ------------------------------------------------------------


		/// <summary>
		/// Sets Eduroam as the chosen network to connect to. Exits the application if Eduroam is not available.
		/// </summary>
		public AvailableNetworkPack SetChosenNetwork()
		{
			// gets all available networks and stores them in a list
			List<AvailableNetworkPack> networks = NativeWifi.EnumerateAvailableNetworks().ToList();

			// sets eduroam as the chosen network,
			// prefers a network with an existing profile
			foreach (AvailableNetworkPack network in networks)
			{
				if (network.Ssid.ToString() == "eduroam")
				{
					foreach (AvailableNetworkPack network2 in networks)
					{
						if (network2.Ssid.ToString() == "eduroam" && network2.ProfileName != "")
						{
							return network2;
						}
					}
					return network;
				}
			}

			// if no networks called "eduroam" are found, return nothing
			return null;
		}

		/// <summary>
		/// Lets user select an XML file containing a wireless network profile.
		/// </summary>
		/// <returns>Path of profile xml.</returns>
		public string GetXmlFile()
		{
			string xmlPath = null;

			OpenFileDialog openXmlDialog = new OpenFileDialog();

			openXmlDialog.InitialDirectory = @"C:\Users\lwerivel18\Documents\Wireless_profiles"; // sets the initial directory of the open file dialog
			openXmlDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*"; // sets filter for file types that appear in open file dialog
			openXmlDialog.FilterIndex = 0;
			openXmlDialog.RestoreDirectory = true;

			if (openXmlDialog.ShowDialog() == DialogResult.OK)
			{
				xmlPath = openXmlDialog.FileName;
			}

			return xmlPath;
		}




		/// <summary>
		/// Gets profile name, SSID name and CA thumbprint and inserts them into a template XML file.
		/// </summary>
		/// <param name="xmlFile">Template XML file path.</param>
		/// <param name="ssid">Profile and SSID name.</param>
		/// <param name="thumb">CA thumbprint.</param>
		/// <returns>Edited profile XML file.</returns>
		public String ConfigureProfileXml(string xmlFile, string ssid, string thumb)
		{
			// loads the XML file from its file path
			XDocument doc = XDocument.Load(xmlFile);

			// shortens namespaces from XML file for easier typing
			XNamespace ns1 = "http://www.microsoft.com/networking/WLAN/profile/v1";
			XNamespace ns2 = "http://www.microsoft.com/networking/OneX/v1";
			XNamespace ns3 = "http://www.microsoft.com/provisioning/EapHostConfig";
			XNamespace ns4 = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
			// namespace changes depending on EAP-type
			XNamespace ns5 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";//(cboMethod.SelectedIndex == 0 ? "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1" : "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1");

			// gets elements to edit from the XML template
			XElement profileName = doc.Root.Element(ns1 + "name");

			XElement ssidName = doc.Root.Element(ns1 + "SSIDConfig")
									.Element(ns1 + "SSID")
									.Element(ns1 + "name");

			XElement thumbprint = doc.Root.Element(ns1 + "MSM")
									 .Element(ns1 + "security")
									 .Element(ns2 + "OneX")
									 .Element(ns2 + "EAPConfig")
									 .Element(ns3 + "EapHostConfig")
									 .Element(ns3 + "Config")
									 .Element(ns4 + "Eap")
									 .Element(ns5 + "EapType")
									 .Element(ns5 + "ServerValidation")
									 .Element(ns5 + "TrustedRootCA");

			// sets elements to desired values
			profileName.Value = ssid;
			ssidName.Value = ssid;
			thumbprint.Value = thumb;

			// adds the xml declaration to the top of the document and converts it to string
			string wDeclaration = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();

			// returns the edited xml file
			return wDeclaration;
		}

		public String ConfigureUserDataXml(string xmlFile, string username, string password)
		{
			// loads the XML file from its file path
			XDocument doc = XDocument.Load(xmlFile);

			// shortens namespaces from XML file for easier typing
			XNamespace ns1 = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
			//XNamespace ns2 = "http://www.microsoft.com/provisioning/EapCommon";
			//XNamespace ns3 = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";

			//XNamespace cr1 = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
			//XNamespace cr2 = "http://www.w3.org/2001/XMLSchema-instance";
			XNamespace cr3 = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";
			XNamespace cr4 = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
			XNamespace cr5 = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";

			XElement xmlUsername = doc.Root.Element(ns1 + "Credentials")
									  .Element(cr3 + "Eap")
									  .Element(cr4 + "EapType")
									  .Element(cr3 + "Eap")
									  .Element(cr5 + "EapType")
									  .Element(cr5 + "Username");

			XElement xmlPassword = doc.Root.Element(ns1 + "Credentials")
									  .Element(cr3 + "Eap")
									  .Element(cr4 + "EapType")
									  .Element(cr3 + "Eap")
									  .Element(cr5 + "EapType")
									  .Element(cr5 + "Password");

			// sets elements to desired values
			xmlUsername.Value = username;
			xmlPassword.Value = password;

			// adds the xml declaration to the top of the document and converts it to string
			string wDeclaration = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();

			return wDeclaration;
		}

		/// <summary>
		/// Creates new network profile according to selected network and profile XML.
		/// </summary>
		/// <param name="networkPack">Info about selected network.</param>
		/// <param name="profileXml">Path of selected profile XML.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public static bool CreateNewProfile(Guid networkID, string profileXml)
		{
			// sets the profile type to be Per User (value = 2)
			// if set to Per User, the security type parameter is not required
			ProfileType newProfileType = ProfileType.AllUser;

			// security type not required
			string newSecurityType = null;

			// overwrites if profile already exists
			bool overwrite = true;

			return NativeWifi.SetProfile(networkID, newProfileType, profileXml, newSecurityType, overwrite);
		}

		// sets a profile's user data (for WPA2-Enterprise networks)
		public static bool SetUserData(Guid networkID, string profileName, string userDataXml)
		{
			uint profileUserType = 0x00000001; // sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"

			var userDataSuccess = NativeWifi.SetProfileUserData(networkID, profileName, profileUserType, userDataXml);
			return userDataSuccess;
		}


		// TEST FUCNTION FOR NEW PROFILE
		public static void TestNewProfile(AvailableNetworkPack networkPack)
		{

			// specifies the path of the XML-file containing the wireless profile
			string xmlPath = @"C:\Users\lwerivel18\Documents\Wireless_profiles\GOODSHIT\PEAP-MSCHAPv2_hard.xml";
			// stores the XML-file in a string
			string xmlString = File.ReadAllText(xmlPath);

			// sets the profile type to be Per User (value = 2)
			// if set to Per User, the security type parameter is not required
			ProfileType newProfileType = ProfileType.AllUser;

			// security type not required
			string newSecurityType = null;

			// overwrites if profile already exists
			bool overwrite = true;

			// lets user know if success
			bool newProfileSuccess = NativeWifi.SetProfile(networkPack.Interface.Id, newProfileType, xmlString, newSecurityType, overwrite);
			Console.WriteLine(newProfileSuccess ? "New profile successfully created." : "Creation of new profile failed.");


		}

		// TEST FUNCTION FOR SET USER DATA
		public static void TestProfileUserData(AvailableNetworkPack networkPack, string inputProfileName)
		{
			var networkInterfaceID = networkPack.Interface.Id; // gets the network interace ID
			string profileName = inputProfileName; // gets the profile name to add the user data to
			uint profileUserType = 0x00000001; // sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"
			// specifies the path of the XML-file containing the profile user data
			string xmlPath = @"C:\Users\lwerivel18\Documents\Wireless_profiles\GOODSHIT\USERDATA_hard.xml";
			// stores the XML-file in a string
			string xmlString = File.ReadAllText(xmlPath);
			Console.WriteLine(xmlString);
			var setUserDataSuccess = NativeWifi.SetProfileUserData(networkInterfaceID, profileName, profileUserType, xmlString);
			Console.WriteLine(setUserDataSuccess ? "User data set correctly." : "Set user data failed.");
		}

		/// <summary>
		/// Downloads and installs a client certificate as well as a Client Authority certificate.
		/// </summary>
		/// <param name="password">The certificate's password.</param>
		/// <returns>Certificate install success, CA install success and CA thumbprint.</returns>
		public Tuple<string, string, string> InstallCertificate(string password)
		{
			string certResult = "Certificate installed successfully.\n";
			string caResult = "CA installed successfully.\n";
			string caThumbprint = null;

			// creates directory to download certificate file to
			string path = $@"{ Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) }\MyCertificates";
			Directory.CreateDirectory(path);
			// sets the file name
			string certFile = "eduroam_certificate.p12";
			string caFile = "Fyrkat+Root+CA.crt";

			// downloads files from server
			//using (WebClient client = new WebClient())
			//{
			//    client.DownloadFile("https://nofile.io/g/JFVD0GuZIlAN75eOSBWPf70HW7ttzarSoI6ToMvqXDNOgdQa1LBIyg1y2inkTlVZ/ericv%40fyrkat.no.p12/", $@"{path}\{certFile}");
			//    client.DownloadFile("https://nofile.io/g/Y42wuToV5J2Cz1lyUdwaAcdrW6LIT6v228YduZFRRhb92sMfQ6zLZK828gHygotI/Fyrkat%2BRoot%2BCA.crt/", $@"{path}\{caFile}");
			//}

			// installs certficate to personal certificate store
			try
			{
				X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadWrite);
				X509Certificate2 certificate = new X509Certificate2($@"{path}\{certFile}", password, X509KeyStorageFlags.PersistKeySet); // include PersistKeySet flag so certificate is valid after reboot


				// checks if certificate is already installed
				var certExist = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true);
				if (certExist != null && certExist.Count > 0)
				{
					certResult = "Certificate already installed.\n";
				}
				else
				{
					store.Add(certificate);
				}
				store.Close(); // closes the certificate store
			}
			catch (Exception)
			{
				certResult = "Certificate installation failed.\n";
			}

			// installs CA to trusted root certificate authority store
			try
			{
				X509Store caStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
				X509Certificate2 ca = new X509Certificate2($@"{path}\{caFile}");
				caStore.Open(OpenFlags.ReadWrite);

				// checks if CA is already installed
				var caExist = caStore.Certificates.Find(X509FindType.FindByThumbprint, ca.Thumbprint, true);
				if (caExist != null && caExist.Count > 0)
				{
					caResult = "CA already installed.\n";
				}
				else
				{
					// show messagebox to let users know about the CA installation warning
					MessageBox.Show("You will now be prompted to install the Certificate Authority. In order to connect to eduroam, you need to accept this by pressing \"Yes\".", "Accept Certificate Authority", MessageBoxButtons.OK);
					caStore.Add(ca);
				}
				caThumbprint = ca.Thumbprint; // gets thumbprint of CA
				caStore.Close(); // closes the certificate store
			}
			catch (Exception)
			{
				caResult = "CA installation failed.\n";
			}

			// returns results of certificate and CA installation, as well as the CA thumbprint
			return Tuple.Create(certResult, caResult, caThumbprint);
		}


		/// <summary>
		/// Connects to the chosen wireless LAN.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		public static async Task<bool> ConnectAsync(AvailableNetworkPack networkParam)
		{
			AvailableNetworkPack chosenWifi = networkParam;

			if (chosenWifi == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: chosenWifi.Interface.Id,
				profileName: chosenWifi.ProfileName,
				bssType: chosenWifi.BssType,
				timeout: TimeSpan.FromSeconds(10));
		}

		private void label4_Click(object sender, EventArgs e)
		{

		}


	}
}
