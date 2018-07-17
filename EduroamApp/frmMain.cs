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
using System.Xml;
using System.Xml.XPath;

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
            // sets default connection method
            cboMethod.SelectedIndex = 0;
        }
        
        private void btnSelectProfile_Click(object sender, EventArgs e)
        {
            string filePath = GetXmlFile();
            string fileName = Path.GetFileName(filePath);
            txtProfilePath.Text = fileName;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // opens dialog to select wireless profile XML
            string profileXml = GetXmlFile(); 

            // stop execution if no profile selected
            if (profileXml == null)
            {
                MessageBox.Show("No file selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOutput.Text += "No profile selected.";
                return;
            }

            string thumbprint = "8d043f808044894db8d8e06da2acf5f98fb4a610"; // default value is server thumbprint(?) for login with username/password

            // sets eduroam as chosen network
            AvailableNetworkPack network = SetChosenNetwork();
            string ssid = network.Ssid.ToString(); // gets SSID
            Guid interfaceID = network.Interface.Id; // gets interface ID

            // CONNECT VIA CERTIFICATE
            if (cboMethod.SelectedIndex == 0)
            {
                // installs client certificate
                //string certPassword = txtCertPwd.Text;
                var getCertificates = GetCertFromString();
                var certificateResult = InstallCertificatesFromFile(getCertificates.Item1, getCertificates.Item2);//InstallCertificate(certPassword);
                txtOutput.Text += certificateResult.Item1; // outputs certificate installation success
                txtOutput.Text += certificateResult.Item2; // outputs CA installation success
                thumbprint = certificateResult.Item3; // gets thumbprint of CA
            }

            // configures profile xml to include ssid name and correct thumbprint
            string newXml = ConfigureProfileXml(profileXml, ssid, thumbprint);            
            // creates a new network profile
            txtOutput.Text += (CreateNewProfile(interfaceID, newXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");
            
            // CONNECT WITH USERNAME+PASSWORD
            if (cboMethod.SelectedIndex == 1)
            {
                // gets user data xml template
                string userDataXml = GetXmlFile();

                // stop execution if no profile selected
                if (userDataXml == null)
                {
                    MessageBox.Show("No file selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtOutput.Text += "User data file not selected.";
                    return;
                }

                // gets username and password from UI
                string username = txtUsername.Text;
                string password = txtPassword.Text;
                // configures user data xml to include username and password
                string newUserData = ConfigureUserDataXml(userDataXml, username, password);
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
        /// Sets eduroam as chosen network to connect to.
        /// </summary>
        /// <returns>Network pack containing eudoram properties.</returns>
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

            openXmlDialog.InitialDirectory = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles\Profile XML"; // sets the initial directory of the open file dialog
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
        /// Gets profile name, SSID name and CA thumbprint and inserts them into a wireless profile XML template.
        /// </summary>
        /// <param name="xmlFile">Path to profile xml template.</param>
        /// <param name="ssid">Profile and SSID name.</param>
        /// <param name="thumb">CA thumbprint.</param>
        /// <returns>Configured XML file as string.</returns>
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
            XNamespace ns5 = (cboMethod.SelectedIndex == 0 ? "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1" : "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1");

            // gets elements to edit
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

        /// <summary>
        /// Inserts username and password into user data XML file.
        /// </summary>
        /// <param name="xmlFile">Path to user data xml template.</param>
        /// <param name="username">User's username.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Configured XML file as string.</returns>
        public String ConfigureUserDataXml(string xmlFile, string username, string password)
        {
            // loads the XML file from its file path
            XDocument doc = XDocument.Load(xmlFile);

            // shortens namespaces from XML file for easier typing
            XNamespace ns1 = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
                        
            XNamespace cr3 = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";
            XNamespace cr4 = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
            XNamespace cr5 = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";

            // gets elements to edit
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
        /// Sets a profile's user data for login with username + password.
        /// </summary>
        /// <param name="networkID">Interface ID of selected network.</param>
        /// <param name="profileName">Name of associated wireless profile.</param>
        /// <param name="userDataXml">User data XML converted to string.</param>
        /// <returns>True if succeeded, false if failed.</returns>
        public static bool SetUserData(Guid networkID, string profileName, string userDataXml)
        {
            // sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"
            uint profileUserType = 0x00000001;

            return NativeWifi.SetProfileUserData(networkID, profileName, profileUserType, userDataXml);
        }

        
        /// <summary>        
        /// Downloads and installs a client certificate and Client Authority certificate.
        /// </summary>
        /// <param name="password">The certificate's password.</param>        
        /// <returns>Certificate install success, CA install success and CA thumbprint.</returns>
        public Tuple<string, string, string> InstallCertificate(string password)
        {
            // declare return values
            string certResult = "Certificate installed successfully.\n";
            string caResult = "CA installed successfully.\n";
            string caThumbprint = null;

            // sets the file path            
            string path = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles\Certificates";
            
            // sets the file names
            string certFile = "eduroam_certificate.p12";
            string caFile = "Fyrkat+Root+CA.crt";
                        
            // installs certficate to personal certificate store
            try
            {
                // opens personal certificate store
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser); 
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2 certificate = new X509Certificate2($@"{path}\{certFile}", password, X509KeyStorageFlags.PersistKeySet); // include PersistKeySet flag so certificate is valid after reboot
                //X509Certificate2 certFromString = GetCertFromString();
                // checks if certificate is already installed
                var certExist = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true);
                if (certExist != null && certExist.Count > 0)
                {
                    certResult = "Certificate already installed.\n";
                }
                else
                {
                    // adds certificate to store
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
                // opens trusted root certificate authority store
                X509Store caStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                caStore.Open(OpenFlags.ReadWrite);
                X509Certificate2 ca = new X509Certificate2($@"{path}\{caFile}");
                
                // checks if CA is already installed
                var caExist = caStore.Certificates.Find(X509FindType.FindByThumbprint, ca.Thumbprint, true);
                if (caExist != null && caExist.Count > 0)
                {
                    caResult = "CA already installed.\n";
                }
                else
                {
                    // show messagebox to let users know about the CA installation warning
                    MessageBox.Show("You will now be prompted to install the Certificate Authority. " +
                                    "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the next dialog box.", "Accept Certificate Authority", MessageBoxButtons.OK);
                    // adds CA to store
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
        /// Reads an EAP config file and gets information about client and server certificates.
        /// </summary>
        /// <returns>Certificate objects.</returns>
        public Tuple<X509Certificate2, X509Certificate2> GetCertFromString()
        {
            // loads the XML file from its file path
            XElement doc = XElement.Load(@"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles\Certificates\fyrkat_EAPConfig.eap-config");
                        
            // shortens namespaces from XML file for easier typing
            XNamespace ns1 = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace ns2 = "urn:RFC4282:realm";

            // gets client certificate as base64
            XElement clientCert = doc.Element("EAPIdentityProvider")
                                            .Element("AuthenticationMethods")
                                            .Element("AuthenticationMethod")
                                            .Element("ClientSideCredential")
                                            .Element("ClientCertificate");

            // gets password for client certificate
            XElement clientCertPwd = doc.Element("EAPIdentityProvider")
                                      .Element("AuthenticationMethods")
                                      .Element("AuthenticationMethod")
                                      .Element("ClientSideCredential")
                                      .Element("Passphrase");

            // gets CA as base64
            XElement xmlCa = doc.Element("EAPIdentityProvider")
                                .Element("AuthenticationMethods")
                                .Element("AuthenticationMethod")
                                .Element("ServerSideCredential")
                                .Element("CA");

            // stores values
            string base64Client = clientCert.Value;
            string certPwd = clientCertPwd.Value;
            string base64Ca = xmlCa.Value;
            
            //decodes the client certificate from base64
            var clientCertBytes = Convert.FromBase64String(base64Client);
            var caBytes = Convert.FromBase64String(base64Ca);

            // creates new certificate object
            X509Certificate2 certificate = new X509Certificate2(clientCertBytes, certPwd, X509KeyStorageFlags.PersistKeySet);
            X509Certificate2 ca = new X509Certificate2(caBytes);
            
            return Tuple.Create(certificate, ca);
        }

        /// <summary>
        /// Gets client certificate and CA and installs in respective certificate stores.
        /// </summary>
        /// <param name="clientCert">Client certificate object.</param>
        /// <param name="ca">CA object.</param>
        /// <returns>Certificate install success, CA install success and CA thumbprint.</returns>
        public Tuple<string, string, string> InstallCertificatesFromFile(X509Certificate2 clientCert, X509Certificate2 ca)
        {
            // declare return values
            string certResult = "Certificate installed successfully.\n";
            string caResult = "CA installed successfully.\n";
            string caThumbprint = null;
                     
            // installs client certficate to personal certificate store
            try
            {
                // opens personal certificate store
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite); 
                
                // checks if certificate is already installed
                var certExist = store.Certificates.Find(X509FindType.FindByThumbprint, clientCert.Thumbprint, true);
                if (certExist != null && certExist.Count > 0)
                {
                    certResult = "Certificate already installed.\n";
                }
                else
                {
                    // adds certificate to store
                    store.Add(clientCert);
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
                // opens trusted root certificate authority store
                X509Store caStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
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
                    MessageBox.Show("You will now be prompted to install the Certificate Authority. " +
                                    "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the next dialog box.", "Accept Certificate Authority", MessageBoxButtons.OK);
                    // adds CA to store
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

        // exits the application
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            GetCertFromString();
        }
    }
}
