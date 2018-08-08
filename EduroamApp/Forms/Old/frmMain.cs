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
using System.Device.Location;

namespace EduroamApp
{
	public partial class frmMain : Form
	{
		// list containing all identity providers
		List<IdentityProvider> identityProviders;
		// list containing all profiles of an identity provider
		IdentityProviderProfile idProviderProfiles;
		// id of selected institution
		int idProviderId;
		// id of selected institution profile
		string profileIdLOL;
		// network pack for eduroam network
		AvailableNetworkPack network;
		// flag indicates wether a client certificate is succesfully installed or not
		int clientCertFlag = 0;
		// gets coordinates of computer
		GeoCoordinateWatcher watcher;


		public frmMain()
		{
			InitializeComponent();

			// starts GeoCoordinateWatcher when app starts
			watcher = new GeoCoordinateWatcher();
			watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
		}

		private void frmMain_load(object sender, EventArgs e)
		{
			// url for json containing all identity providers / institutions
			string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";

			// json file as string
			string idProviderJson = "";
			try
			{
				idProviderJson = UrlToJson(allIdentityProvidersUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider list. \nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
				//Application.Exit();
			}

			// gets list of identity providers from json file
			identityProviders = JsonConvert.DeserializeObject<List<IdentityProvider>>(idProviderJson);
			// adds countries to combobox
			cboCountry.Items.AddRange(identityProviders.OrderBy(provider => provider.Country).Select(provider => provider.Country).Distinct().ToArray());

			try
			{
				string closestCountry = GetClosestInstitution(identityProviders);
				cboCountry.SelectedIndex = cboCountry.FindStringExact(closestCountry);
			}
			catch (System.Exception ex)
			{
				txtOutput.Text += "Geolocation not set. \nException: " + ex.Message + "\n";
			}
		}

		private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboInstitution.Items.Clear();
			cboProfiles.Items.Clear();
			// clear selected profile
			profileIdLOL = null;

			// adds identity providers from selected country to combobox
			cboInstitution.Items.AddRange(identityProviders.Where(provider => provider.Country == cboCountry.Text).OrderBy(provider => provider.Title).Select(provider => provider.Title).ToArray());
		}

		private void cboInstitution_SelectedIndexChanged(object sender, EventArgs e)
		{
			// clear combobox
			cboProfiles.Items.Clear();
			// clear selected profile
			profileIdLOL = null;

			// gets id of institution selected in combobox
			idProviderId = identityProviders.Where(x => x.Title == cboInstitution.Text).Select(x => x.Id).First();
			// adds institution id to url
			string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={idProviderId}&lang=en";

			// json file as string
			string profilesJson = "";
			try
			{
				profilesJson = UrlToJson(profilesUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch identity provider profiles.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// gets identity provider profile from json
			idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);

			// if an identity provider has more than one profile, add to combobox
			if (idProviderProfiles.Data.Count > 1)
			{
				// enable combobox
				cboProfiles.Enabled = true;
				// enable label
				lblSelectProfile.Enabled = true;
				// add profiles to combobox
				cboProfiles.Items.AddRange(idProviderProfiles.Data.Select(profile => profile.Display).ToArray());
			}
			else
			{
				// gets the only profile id
				profileIdLOL = idProviderProfiles.Data.Single().Id;
				// disable combobox
				cboProfiles.Enabled = false;
				// disable label
				lblSelectProfile.Enabled = false;
			}
		}

		private void cboProfiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboProfiles.Text != "")
			{
				// gets profile id of profile selected in combobox
				profileIdLOL = idProviderProfiles.Data.Where(profile => profile.Display == cboProfiles.Text).Select(x => x.Id).Single();
			}
		}


		private void btnDownloadEap_Click(object sender, EventArgs e)
		{
			// checks if user has selected an institution and/or profile
			if (string.IsNullOrEmpty(profileIdLOL))
			{
				txtOutput.Text += "No institution or profile selected.\n";
				return; // exits function if no institution/profile selected
			}

			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileIdLOL}";

			// json file
			string generateEapJson;
			// gets json as string
			try
			{
				generateEapJson = UrlToJson(generateEapUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config generate.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// converts json to GenerateEapConfig object
			GenerateEapConfig eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);

			// gets url to EAP config file download from GenerateEapConfig object
			string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

			// eap config file
			string eapConfigString = "";
			// gets eap config file as string
			try
			{
				eapConfigString = UrlToJson(eapConfigUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config file.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			txtOutput.Text += "EAP config file ready.\n";
			//MessageBox.Show(eapConfigString);
			/* Connect(eapConfigString); */
		}

		private void btnLocalEap_Click(object sender, EventArgs e)
		{
			// opens dialog to select EAP Config file
			string eapConfigPath = GetFileFromDialog("Select EAP Config file");
			// cancel and present error message if no file selected
			if (validateFileSelection(eapConfigPath) == false) { return; }

			string eapConfigString = File.ReadAllText(eapConfigPath);
			Connect(eapConfigString);
		}

		// ------------------------------------------------------------- FUNCIONS --------------------------------------------------------------------------


		public void Connect(string eapString)
		{
			// sets eduroam as chosen network
			EduroamNetwork eduroamInstance = new EduroamNetwork(); // creates new instance of eduroam network
			network = eduroamInstance.networkPack; // gets network pack
			string ssid = eduroamInstance.ssid; // gets SSID
			Guid interfaceId = eduroamInstance.interfaceId; // gets interface ID

			// all CA thumbprints that will be added to Wireless Profile XML
			List<string> thumbprints = new List<string>();
			//thumbprints.Add("8d043f808044894db8d8e06da2acf5f98fb4a610"); //thumbprint for login with username/password
			thumbprints.Add("5a6b4ec8b86e5aad0f539821889c23cd64d32cf7"); //thumbprint for login with username/password


			// gets a list of all client certificates and a list of all CAs
			var getAllCertificates = GetCertificates(eapString);
			// opens trusted root certificate authority store
			X509Store store;

			// checks if there are any certificates to install
			if (getAllCertificates.Item1.Any())
			{
				// sets store to personal certificate store
				store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

				// loops through list and attempts to install client certs
				foreach (X509Certificate2 clientCert in getAllCertificates.Item1)
				{
					try
					{
						// installs client certificate
						InstallCertificate(clientCert, store);
						// outputs result
						txtOutput.Text += $"Certificate installed: {clientCert.FriendlyName}\n";
						// sets flag
						clientCertFlag = 1;
					}
					catch (CryptographicException ex)
					{
						// outputs error message if a certificate installation fails
						txtOutput.Text += $"Certificate was not installed: {clientCert.FriendlyName}\nError: {ex.Message}";
					}
				}
			}
			else
			{
				// if no certs to install, flag set to 0
				// user will have to authenticate with username and password instead
				clientCertFlag = 0;
			}

			// checks if there are CAs to install
			if (getAllCertificates.Item2.Any())
			{
				// sets store to trusted root certificate store
				store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

				// loops through list and attempts to install CAs
				foreach (X509Certificate2 certAuth in getAllCertificates.Item2)
				{
					try
					{
						// installs client certificate
						InstallCertificate(certAuth,store);
						// outputs result
						txtOutput.Text += $"CA installed: {certAuth.FriendlyName}\n";
						// adds thumbprint to list
						thumbprints.Add(certAuth.Thumbprint);
					}
					catch (CryptographicException ex)
					{
						// outputs error message if a certificate installation fails
						txtOutput.Text += $"CA was not installed: {certAuth.FriendlyName}\nError: {ex.Message}";
					}
				}
			}

			// sets chosen EAP-type based on wether certificate was successfully installed
			uint eapType = 0;

			// generates new profile xml
			string profileXml = ProfileXml.CreateProfileXml(ssid, eapType, "HI", thumbprints);

			// creates a new wireless profile
			txtOutput.Text += (CreateNewProfile(interfaceId, profileXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");

			eduroamInstance = new EduroamNetwork(); // creates new instance of eduroam network
			network = eduroamInstance.networkPack; // gets updated network pack object

			// CONNECT WITH USERNAME+PASSWORD
			if (clientCertFlag == 0)
			{
				// if no certificate gets installed, prompts for username and password
				DialogResult loginWithCredentials = MessageBox.Show("A certificate could not be installed. \nDo you want to log in with your eduroam credentials instead?", "Certificate not installed", MessageBoxButtons.YesNo);
				if (loginWithCredentials == DialogResult.Yes)
				{
					var logonForm = new frmLogon();
					// passes the current network pack as a parameter
					logonForm.GetEduroamInstance(network);
					// shows the logon form as a dialog, so user can't interact with main form
					logonForm.ShowDialog();
				}
			}
			else
			{
				// connects to eduroam
				var connectResult = Task.Run(() => ConnectAsync(network)).Result;
				txtOutput.Text += (connectResult ? "You are now connected to " + ssid + ".\n" : "Connection failed.\n");
			}
		}

		/// <summary>
		/// Gets a json file as string from url.
		/// </summary>
		/// <param name="url">Url containing json file.</param>
		/// <returns>Json string.</returns>
		public string UrlToJson(string url)
		{
			// downloads json file from url as string
			using (WebClient client = new WebClient())
			{
				string jsonString = client.DownloadString(url);
				return jsonString;
			}
		}

		/// <summary>
		/// Compares institution coordinates with user's coordinates and gets the closest institution.
		/// </summary>
		/// <param name="instList">List of all institutions.</param>
		/// <returns>Country of closest institution.</returns>
		public string GetClosestInstitution(List<IdentityProvider> instList)
		{
			// user's coordinates
			GeoCoordinate myCoord = watcher.Position.Location;
			// institution's coordinates
			GeoCoordinate instCoord = new GeoCoordinate();
			// current distance
			// closest institution
			IdentityProvider closestInst = new IdentityProvider();
			// shortest distance
			double shortestDistance = double.MaxValue;

			// loops through all institutions' coordinates and compares them with current shortest distance
			foreach (IdentityProvider inst in instList)
			{
				if (inst.Geo == null) continue;
				// gets lat and long
				instCoord.Latitude = (double) inst.Geo.First().Lat;
				instCoord.Longitude = (double) inst.Geo.First().Lon;
				// gets distance
				double currentDistance = myCoord.GetDistanceTo(instCoord);
				// compares with current shortest distance
				if (!(currentDistance < shortestDistance)) continue;
				shortestDistance = currentDistance;
				closestInst = inst;
			}

			// returns country of institution closest to user
			return closestInst.Country;
		}

		/// <summary>
		/// Creates new network profile according to selected network and profile XML.
		/// </summary>
		/// <param name="networkID">Interface ID of selected network.</param>
		/// <param name="profileXml">Wireless profile XML converted to string.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public static bool CreateNewProfile(Guid networkID, string profileXml)
		{
			// sets the profile type to be All-user (value = 0)
			// if set to Per User, the security type parameter is not required
			ProfileType newProfileType = ProfileType.AllUser;

			// security type not required
			string newSecurityType = null;

			// overwrites if profile already exists
			bool overwrite = true;

			return NativeWifi.SetProfile(networkID, newProfileType, profileXml, newSecurityType, overwrite);
		}

		/// <summary>
		/// Gets all client certificates and CAs from EAP config file.
		/// </summary>
		/// <param name="filePath">Filepath of EAP Config file.</param>
		/// <returns>List of client certificates and list of CAs.</returns>
		public Tuple<List<X509Certificate2>, List<X509Certificate2>> GetCertificates(string fileString)
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
						string clientPwd = el.DescendantsAndSelf().Elements().FirstOrDefault(pw => pw.Name.LocalName == "Passphrase").Value; // Client cert password
						// converts from base64
						var clientBytes = Convert.FromBase64String(base64Client); // Client cert decoded from base64
						// creates certificate object
						X509Certificate2 clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet); // Client cert object
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
		public void InstallCertificate(X509Certificate2 cert, X509Store store)
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

		/// <summary>
		/// Connects to the chosen wireless LAN.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		public static async Task<bool> ConnectAsync(AvailableNetworkPack chosenWifi)
		{
			if (chosenWifi == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: chosenWifi.Interface.Id,
				profileName: chosenWifi.ProfileName,
				bssType: chosenWifi.BssType,
				timeout: TimeSpan.FromSeconds(10));
		}

		/// <summary>
		/// Lets user select a file through an OpenFileDialog.
		/// </summary>
		/// <param name="dialogTitle">Title of the OpenFileDialog.</param>
		/// <returns>Path of selected file.</returns>
		public string GetFileFromDialog(string dialogTitle)
		{
			string filePath = null;

			OpenFileDialog fileDialog = new OpenFileDialog
			{
				InitialDirectory = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles",
				Filter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*",
				FilterIndex = 0,
				RestoreDirectory = true,
				Title = dialogTitle
			};

			// sets the initial directory of the open file dialog
			// sets filter for file types that appear in open file dialog

			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				filePath = fileDialog.FileName;
			}

			return filePath;
		}

		/// <summary>
		/// Checks wether a file is chosen during an open file dialog.
		/// </summary>
		/// <param name="filePath">Filepath returned from open file dialog.</param>
		/// <returns>True if valid filepath, false if not.</returns>
		public bool validateFileSelection(string filePath)
		{
			if (filePath == null)
			{
				MessageBox.Show("No file selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			else if (Path.GetExtension(filePath) != ".eap-config")
			{
				MessageBox.Show("File type not supported.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			return true;
		}

		// exits the application
		private void btnExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		// test functions
		private void btnTest_Click(object sender, EventArgs e)
		{
			List<uint> eapTypes = new List<uint>();

			// gets id of institution selected in combobox
			IEnumerable<int> providerIds = identityProviders.Select(x => x.Id);

			foreach (int providerId in providerIds)
			{
				// adds institution id to url
				string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={providerId}&lang=en";

				// json file as string
				try
				{
					string profilesJson = UrlToJson(profilesUrl);
					idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);
				}
				catch (Exception ex)
				{
					txtOutput.Text += "Couldn't fetch identity provider profiles.\nException: " + ex.Message + "\n";
				}

				// gets profile id of profile selected in combobox
				IEnumerable<string> profileIds = idProviderProfiles.Data.Where(profile => profile.Display == cboProfiles.Text).Select(x => x.Id);

				//-------------------------------------------------

				foreach (string profileId in profileIds)
				{
					// adds profile ID to url containing json file, which in turn contains url to EAP config file download
					string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileId}";
					GenerateEapConfig eapConfigInstance = new GenerateEapConfig();

					// gets json as string
					try
					{
						string generateEapJson = UrlToJson(generateEapUrl);
						// converts json to GenerateEapConfig object
						eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);
					}
					catch (Exception ex)
					{
						txtOutput.Text += "Couldn't fetch Eap Config generate.\nException: " + ex.Message + "\n";
					}


					// gets url to EAP config file download from GenerateEapConfig object
					string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

					// eap config file
					string eapConfigString = "";
					// gets eap config file as string
					try
					{
						eapConfigString = UrlToJson(eapConfigUrl);
					}
					catch (Exception ex)
					{
						txtOutput.Text += "Couldn't fetch Eap Config file.\nException: " + ex.Message + "\n";
					}

					foreach (uint eapType in GetEapType(eapConfigString))
					{
						eapTypes.Add(eapType);
					}
				}
			}

			List<uint> distinctEapTypes = eapTypes.Distinct().ToList();

			txtOutput.Text += "DISTINCT EAP TYPES:\n";

			foreach (uint distinctEapType in distinctEapTypes)
			{
				txtOutput.Text += distinctEapType + "\n";
			}
		}

		/// <summary>
		/// Gets all eap types from EAP config file.
		/// </summary>
		/// <param name="eapFile">EAP config file as string.</param>
		/// <returns>List of eap types</returns>
		public List<uint> GetEapType(string eapFile)
		{
			// loads the XML file from its file path
			XElement doc = XElement.Parse(eapFile);

			// gets all AuthenticationMethods elements
			IEnumerable<XElement> authMethodElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "AuthenticationMethod");
			List<uint> eapTypes = new List<uint>();

			foreach (XElement element in authMethodElements)
			{
				// gets EAP method type
				uint eapType = (uint)element.DescendantsAndSelf().Elements().First(x => x.Name.LocalName == "Type");
				eapTypes.Add(eapType);
			}

			return eapTypes;
		}

		private void btnTest2_Click(object sender, EventArgs e)
		{
			// checks if user has selected an institution and/or profile
			if (string.IsNullOrEmpty(profileIdLOL))
			{
				txtOutput.Text += "No institution or profile selected.\n";
				return; // exits function if no institution/profile selected
			}

			// adds profile ID to url containing json file, which in turn contains url to EAP config file download
			string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileIdLOL}";

			// json file
			string generateEapJson;
			// gets json as string
			try
			{
				generateEapJson = UrlToJson(generateEapUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config generate.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// converts json to GenerateEapConfig object
			GenerateEapConfig eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);

			// gets url to EAP config file download from GenerateEapConfig object
			string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.Data.Link}";

			// eap config file
			string eapConfigString = "";
			// gets eap config file as string
			try
			{
				eapConfigString = UrlToJson(eapConfigUrl);
			}
			catch (WebException ex)
			{
				MessageBox.Show("Couldn't fetch Eap Config file.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			foreach (uint eapType in GetEapType(eapConfigString))
			{
				txtOutput.Text += eapType + "\n";
			}
		}
	}
}
