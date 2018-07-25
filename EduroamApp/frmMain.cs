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
        string profileId;
        // network pack for eduroam network
        AvailableNetworkPack network;
        // flag indicates wether a client certificate is succesfully installed or not
        int clientCertFlag = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_load(object sender, EventArgs e)
        {                      
            // url for json containing all identity providers / institutions
            string allIdentityProvidersUrl = "https://cat.eduroam.org/user/API.php?action=listAllIdentityProviders&lang=en";

            // json file as string
            string idProviderJson = "";
            try
            {
                idProviderJson = urlToJson(allIdentityProvidersUrl);
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
            cboCountry.Items.AddRange(identityProviders.OrderBy(provider => provider.country).Select(provider => provider.country).Distinct().ToArray());
        }

        private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            // clear combobox
            cboInstitution.Items.Clear();
            cboProfiles.Items.Clear();
            // clear selected profile
            profileId = null;

            // adds identity providers from selected country to combobox
            cboInstitution.Items.AddRange(identityProviders.Where(provider => provider.country == cboCountry.Text).OrderBy(provider => provider.title).Select(provider => provider.title).ToArray());
        }

        private void cboInstitution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // clear combobox
            cboProfiles.Items.Clear();
            // clear selected profile
            profileId = null;

            // gets id of institution selected in combobox
            idProviderId = identityProviders.Where(x => x.title == cboInstitution.Text).Select(x => x.id).First();
            // adds institution id to url
            string profilesUrl = $"https://cat.eduroam.org/user/API.php?action=listProfiles&id={idProviderId}&lang=en";

            // json file as string
            string profilesJson = "";
            try
            {
                profilesJson = urlToJson(profilesUrl);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Couldn't fetch identity provider profiles.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // gets identity provider profile from json
            idProviderProfiles = JsonConvert.DeserializeObject<IdentityProviderProfile>(profilesJson);

            // if an identity provider has more than one profile, add to combobox
            if (idProviderProfiles.data.Count > 1)
            {
                // enable combobox
                cboProfiles.Enabled = true;
                // enable label
                lblSelectProfile.Enabled = true;
                // add profiles to combobox
                cboProfiles.Items.AddRange(idProviderProfiles.data.Select(profile => profile.display).ToArray());
            }
            else
            {
                // gets the only profile id
                profileId = idProviderProfiles.data.Single().id;
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
                profileId = idProviderProfiles.data.Where(profile => profile.display == cboProfiles.Text).Select(x => x.id).Single();
            }
        }


        private void btnDownloadEap_Click(object sender, EventArgs e)
        {
            // checks if user has selected an institution and/or profile
            if (profileId == null || profileId == "")
            {
                txtOutput.Text += "No institution or profile selected.\n";
                return; // exits function if no institution/profile selected
            }

            // adds profile ID to url containing json file, which in turn contains url to EAP config file download
            string generateEapUrl = $"https://cat.eduroam.org/user/API.php?action=generateInstaller&id=eap-config&lang=en&profile={profileId}";

            // json file
            string generateEapJson = "";
            // gets json as string
            try
            {
                generateEapJson = urlToJson(generateEapUrl);
            }
            catch (WebException ex)
            {
                MessageBox.Show("Couldn't fetch Eap Config generate.\nException: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // converts json to GenerateEapConfig object
            GenerateEapConfig eapConfigInstance = JsonConvert.DeserializeObject<GenerateEapConfig>(generateEapJson);

            // gets url to EAP config file download from GenerateEapConfig object
            string eapConfigUrl = $"https://cat.eduroam.org/user/{eapConfigInstance.data.link}";

            // eap config file
            string eapConfigString = "";
            // gets eap config file as string
            try
            {
                eapConfigString = urlToJson(eapConfigUrl);
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

        // -------------------------------------------- FUNCIONS ------------------------------------------------------------


        public void Connect(string eapString)
        {
            // sets eduroam as chosen network
            EduroamNetwork eduroamInstance = new EduroamNetwork(); // creates new instance of eduroam network
            network = eduroamInstance.networkPack; // gets network pack
            string ssid = eduroamInstance.ssid; // gets SSID
            Guid interfaceID = eduroamInstance.interfaceId; // gets interface ID

            // all CA thumbprints that will be added to Wireless Profile XML
            List<string> thumbprints = new List<string>();
            thumbprints.Add("8d043f808044894db8d8e06da2acf5f98fb4a610"); //thumbprint for login with username/password
                

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
            ProfileXml.EapType eapType;
            if (clientCertFlag == 1) { eapType = ProfileXml.EapType.TLS; } else { eapType = ProfileXml.EapType.PEAP_MSCHAPv2; };

            // generates new profile xml
            string profileXml = ProfileXml.CreateProfileXml(ssid, eapType, thumbprints);

            // creates a new wireless profile
            txtOutput.Text += (CreateNewProfile(interfaceID, profileXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");

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
        public string urlToJson(string url)
        {            
            // downloads json file from url as string
            using (WebClient client = new WebClient())
            {
                string jsonString = client.DownloadString(url);
                return jsonString;
            }
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

            string base64Client = null; // Client cert encoded to base64
            byte[] clientBytes = null; // Client cert decoded from base64
            string clientPwd = null; // Client cert password
            X509Certificate2 clientCert = null; // Client cert object

            string base64Ca = null; // CA encoded to base64
            byte[] caBytes = null; // CA decoded from base64
            X509Certificate2 caCert = null; // CA object            

            
            // certificate lists to be populated
            List<X509Certificate2> clientCertificates = new List<X509Certificate2>();
            List<X509Certificate2> certAuthorities = new List<X509Certificate2>();

            // gets all ClientSideCredential elements
            IEnumerable<XElement> clientCredElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientSideCredential");
            IEnumerable<XElement> certElements = null; // list of ClientCertificate elements
            IEnumerable<XElement> caElements = null; // list of CA elements

            // gets client certificates and adds them to client certificate list
            foreach (XElement el in clientCredElements)
            {
                // checks every ClientSideCredential element for ClientCertificate elements
                certElements = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientCertificate");
                if (certElements.Any())
                {
                    // there should only be one ClientCertificate in a ClientSideCredential element, so gets the first one
                    base64Client = certElements.First().Value;
                    if (base64Client != "") // checks that the certificate value is not empty
                    {
                        // gets passphrase element
                        clientPwd = el.DescendantsAndSelf().Elements().Where(pw => pw.Name.LocalName == "Passphrase").FirstOrDefault().Value;
                        // converts from base64
                        clientBytes = Convert.FromBase64String(base64Client);
                        // creates certificate object
                        clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
                        // sets friendly name of certificate
                        clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);
                        // adds certificate object to list
                        clientCertificates.Add(clientCert);
                    }                    
                }                
            }

            // gets CAs and adds them to CA list
            caElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "CA");
            foreach (XElement ca in caElements)
            {
                base64Ca = ca.Value;
                if (base64Ca != "") // checks that the CA value is not empty
                {
                    // converts from base64
                    caBytes = Convert.FromBase64String(base64Ca);
                    // creates certificate object
                    caCert = new X509Certificate2(caBytes);
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
        /// Installs client certificate in personal certificate store.
        /// </summary>
        /// <param name="cert">Certificate object.</param>
        public void InstallClientCertificate(X509Certificate2 cert)
        {                                                
            // opens personal certificate store
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            // adds certificate to store
            store.Add(cert);
                        
            // closes personal certificate store
            store.Close();
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
                if (certExists == null || certExists.Count < 1)
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

            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.InitialDirectory = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles"; // sets the initial directory of the open file dialog
            fileDialog.Filter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*"; // sets filter for file types that appear in open file dialog
            fileDialog.FilterIndex = 0;
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = dialogTitle;

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

            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

            // Do not suppress prompt, and wait 1000 milliseconds to start.
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            watcher.Start();

            GeoCoordinate coord = watcher.Position.Location;

            if (coord.IsUnknown != true)
            {
                MessageBox.Show($"Lat: {coord.Latitude}, Long: {coord.Longitude}");
            }
            else
            {
                MessageBox.Show("Unknown latitude and longitude.");
            }
        }

        
    }
}
