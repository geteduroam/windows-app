﻿using System;
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
            string filePath = GetXmlFile("Select Wireless Profile");
            
            string fileName = Path.GetFileName(filePath);
            txtProfilePath.Text = fileName;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {           
            // all CA thumbprints that will be added to Wireless Profile XML
            List<string> thumbprints = new List<string>();

            //thumbprint for login with username/password
            thumbprints.Add("8d043f808044894db8d8e06da2acf5f98fb4a610"); 

            // sets eduroam as chosen network
            AvailableNetworkPack network = SetChosenNetwork();
            string ssid = network.Ssid.ToString(); // gets SSID
            Guid interfaceID = network.Interface.Id; // gets interface ID

            // CONNECT VIA CERTIFICATE
            if (cboMethod.SelectedIndex == 0)
            {               
                // opens dialog to select EAP Config file
                string eapConfigPath = GetXmlFile("Select EAP Config file");
                // validates that file has been selected
                if (validateFileSelection(eapConfigPath) == false) { return; } 

                // gets a list of all client certificates and a list of all CAs
                var getAllCertificates = GetCertificates(eapConfigPath);

                // installs client certs
                foreach (X509Certificate2 clientCert in getAllCertificates.Item1)
                {
                    // outputs result
                    txtOutput.Text += InstallClientCertificate(clientCert);
                }
                // installs CAs
                foreach (X509Certificate2 certAuth in getAllCertificates.Item2)
                {
                    var caResults = InstallCA(certAuth);
                    // outputs result
                    txtOutput.Text += caResults.Item1;
                    // adds CA thumbprint to list if not null
                    if (caResults.Item2 != null) { thumbprints.Add(caResults.Item2); }                    
                }
            }
            

            // opens dialog to select wireless profile XML
            string profileXml = GetXmlFile("Select Wireless Profile");
            // validates that a file has been selected
            if (validateFileSelection(profileXml) == false) { return; }
            
            // configures profile xml to include ssid name and correct thumbprint
            string newXml = ConfigureProfileXml(profileXml, ssid, thumbprints);            
            // creates a new network profile
            txtOutput.Text += (CreateNewProfile(interfaceID, newXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");
            
            // CONNECT WITH USERNAME+PASSWORD
            if (cboMethod.SelectedIndex == 1)
            {
                // opens dialog to select user data XML
                string userDataXml = GetXmlFile("Select User Data XML");
                // validates that a file has been selected
                if (validateFileSelection(userDataXml) == false) { return; }

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
        public string GetXmlFile(string dialogTitle)
        {
            string xmlPath = null;

            OpenFileDialog openXmlDialog = new OpenFileDialog();

            openXmlDialog.InitialDirectory = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles"; // sets the initial directory of the open file dialog
            openXmlDialog.Filter = "All files (*.*)|*.*"; // sets filter for file types that appear in open file dialog
            openXmlDialog.FilterIndex = 0;
            openXmlDialog.RestoreDirectory = true;
            openXmlDialog.Title = dialogTitle;

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
        public String ConfigureProfileXml(string xmlFile, string ssid, List<string> thumbprints)
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

            XElement serverValidationElement = doc.Root.Element(ns1 + "MSM")
                                     .Element(ns1 + "security")
                                     .Element(ns2 + "OneX")
                                     .Element(ns2 + "EAPConfig")
                                     .Element(ns3 + "EapHostConfig")
                                     .Element(ns3 + "Config")
                                     .Element(ns4 + "Eap")
                                     .Element(ns5 + "EapType")
                                     .Element(ns5 + "ServerValidation");
                                     //.Element(ns5 + "TrustedRootCA");

            // sets elements to desired values
            profileName.Value = ssid;
            ssidName.Value = ssid;
            
            foreach (string thumb in thumbprints)
            {
                serverValidationElement.Add(new XElement(ns5 + "TrustedRootCA", thumb));
            }

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
            XElement clientCert = doc.DescendantsAndSelf().Elements().Where(d => d.Name.LocalName == "ClientCertificate").FirstOrDefault();

            // gets password for client certificate
            XElement clientCertPwd = doc.DescendantsAndSelf().Elements().Where(d => d.Name.LocalName == "Passphrase").FirstOrDefault();

            // gets CA as base64
            XElement xmlCa = doc.DescendantsAndSelf().Elements().Where(d => d.Name.LocalName == "CA").FirstOrDefault();

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
        /// Installs a client certificate.
        /// </summary>
        /// <param name="cert">Client certificate object.</param>
        /// <returns>Result of certificate installation./returns>
        public string InstallClientCertificate(X509Certificate2 cert)
        {
            // gets certificate issuer
            string certIssuer = cert.Issuer;
            // return string
            string certResult = "Certificate installed successfully: ";

            // installs client certficate to personal certificate store
            try
            {
                // opens personal certificate store
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);

                // checks if certificate is already installed
                var certExist = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, true);
                if (certExist != null && certExist.Count > 0)
                {
                    certResult = "Certificate already installed: ";
                }
                else
                {
                    // adds certificate to store
                    store.Add(cert);
                }

                // closes personal certificate store
                store.Close(); 
            }
            catch (Exception)
            {
                certResult = "Certificate installation failed: ";
            }

            return certResult + certIssuer + "\n";
        }

        /// <summary>
        /// Installs a certificate authority.
        /// </summary>
        /// <param name="ca">Certificate authority object.</param>
        /// <returns>Result of certificate installation and thumbprint.</returns>
        public Tuple<string, string> InstallCA(X509Certificate2 ca)
        {
            // gets certificate issuer
            string certIssuer = ca.Issuer;
            // sets return string
            string certResult = "CA installed successfully: ";

            // thumbprint is updated if certificate install is successful
            string certThumbprint = null;
            

            // installs client certficate to personal certificate store
            try
            {
                // opens trusted root certificate authority store
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);

                // checks if CA is already installed
                var certExist = store.Certificates.Find(X509FindType.FindByThumbprint, ca.Thumbprint, true);
                if (certExist != null && certExist.Count > 0)
                {
                    // updates return string
                    certResult = "CA already installed: ";
                }
                else
                {
                    // show messagebox to let users know about the CA installation warning
                    MessageBox.Show("You will now be prompted to install the Certificate Authority. " +
                                    "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.", "Accept Certificate Authority", MessageBoxButtons.OK);
                    // adds CA to store
                    store.Add(ca);
                }
                // closes trusted root certificate authority store
                store.Close();
                certThumbprint = ca.Thumbprint;
            }
            catch (Exception)
            {
                // updates return string
                certResult = "CA installation failed: ";
            }

            certResult += certIssuer + "\n";

            // returns two values: return message and thumbprint
            return Tuple.Create(certResult, certThumbprint);
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
            
        }

        /// <summary>
        /// Gets all client certificates and CAs from EAP config file.
        /// </summary>
        /// <param name="filePath">Filepath of EAP Config file.</param>
        /// <returns>List of client certificates and list of CAs.</returns>
        public Tuple<List<X509Certificate2>, List<X509Certificate2>> GetCertificates(string filePath)
        {
            // loads the XML file from its file path
            XElement doc = XElement.Load(filePath);

            string base64Client = null; // Client cert encoded to base64
            byte[] clientBytes = null; // Client cert decoded from base64
            string clientPwd = null; // Client cert password
            X509Certificate2 clientCert = null; // Client cert object

            string base64Ca = null; // CA encoded to base64
            byte[] caBytes = null; // CA decoded from base64
            X509Certificate2 ca = null; // CA object            

            // gets all AuthenticationMethod elements
            IEnumerable<XElement> authMethodElements = doc.DescendantsAndSelf().Elements().Where(au => au.Name.LocalName == "AuthenticationMethod");
            IEnumerable<XElement> caElements = null;

            // certificate lists to be populated
            List<X509Certificate2> clientCertificates = new List<X509Certificate2>();
            List<X509Certificate2> certAuthorities = new List<X509Certificate2>();

            // gets client certificates and CAs and adds them to their respective lists
            foreach (XElement el in authMethodElements)
            {
                // AuthenticationMethod element only has one ClientCertificate element, so gets first
                base64Client = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientCertificate").FirstOrDefault().Value;
                if (base64Client != "") // excludes if empty
                {
                    // gets passphrase element
                    clientPwd = el.DescendantsAndSelf().Elements().Where(pw => pw.Name.LocalName == "Passphrase").FirstOrDefault().Value;
                    // converts from base64
                    clientBytes = Convert.FromBase64String(base64Client);
                    // creates certificate object
                    clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
                    // adds certificate object to list
                    clientCertificates.Add(clientCert);
                }  

                // AuthenticationMethod element can have multiple CA elements, so loops through them
                caElements = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "CA");
                foreach (XElement caElement in caElements)
                {
                    base64Ca = caElement.Value;
                    if (base64Ca != "") // excludes if empty
                    {
                        // converts from base64
                        caBytes = Convert.FromBase64String(base64Ca);
                        // creates certificate object
                        ca = new X509Certificate2(caBytes);
                        // adds certificate object to list
                        certAuthorities.Add(ca);
                    }
                }                
            }            
            // returns both certificate lists
            return Tuple.Create(clientCertificates, certAuthorities);
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
                txtOutput.Text += "No file selected.";
                return false;
            }
            return true;
        }
    }
}
