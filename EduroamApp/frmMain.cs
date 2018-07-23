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
        // network pack for eduroam network
        AvailableNetworkPack network;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_load(object sender, EventArgs e)
        {
            // sets default connection method
            cboMethod.SelectedIndex = 0;
        }
                

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // sets eduroam as chosen network
            EduroamNetwork eduroamInstance = new EduroamNetwork(); // creates new instance of eduroam network
            network = eduroamInstance.networkPack; // gets network pack
            string ssid = eduroamInstance.ssid; // gets SSID
            Guid interfaceID = eduroamInstance.interfaceId; // gets interface ID

            // all CA thumbprints that will be added to Wireless Profile XML
            List<string> thumbprints = new List<string>();
            thumbprints.Add("8d043f808044894db8d8e06da2acf5f98fb4a610"); //thumbprint for login with username/password
                                   

            int clientCertFlag = 0;
            int caFlag = 0;

            // opens dialog to select EAP Config file
            string eapConfigPath = GetXmlFile("Select EAP Config file");
            // validates that file has been selected
            if (validateFileSelection(eapConfigPath) == false) { return; }

            // gets a list of all client certificates and a list of all CAs
            var getAllCertificates = GetCertificates(eapConfigPath);

            // checks if there are certificates to install
            if (getAllCertificates.Item1.Any())
            {
                // installs client certs
                foreach (X509Certificate2 clientCert in getAllCertificates.Item1)
                {
                    // outputs result
                    txtOutput.Text += InstallClientCertificate(clientCert);
                }
                clientCertFlag = 1;
            }
            else
            {
                //MessageBox.Show("No certificates could be found, please login with username and password.");
                clientCertFlag = 0;
            }

            // checks if there are CAs to install
            if (getAllCertificates.Item2.Any())
            {
                // installs CAs
                foreach (X509Certificate2 certAuth in getAllCertificates.Item2)
                {
                    var caResults = InstallCA(certAuth);
                    // outputs result
                    txtOutput.Text += caResults.Item1;
                    // adds CA thumbprint to list if not null
                    if (caResults.Item2 != null) { thumbprints.Add(caResults.Item2); }
                }
                caFlag = 1;
            }
            else
            {
                caFlag = 0;
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
                    logonForm.GetEduroamInstance(network);
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

        // lets user select wether they want to connect to eduroam using a certificate or username and password
        private void cboMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedMethod = cboMethod.SelectedIndex;

            // shows/hides controls according to chosen connection method
            switch (selectedMethod)
            {
                case 0:
                    lblUsername.Enabled = false;
                    lblPassword.Enabled = false;
                    txtUsername.Enabled = false;
                    txtPassword.Enabled = false;
                    break;
                case 1:
                    lblUsername.Enabled = true;
                    lblPassword.Enabled = true;
                    txtUsername.Enabled = true;
                    txtPassword.Enabled = true;
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
            X509Certificate2 caCert = null; // CA object            

            
            // certificate lists to be populated
            List<X509Certificate2> clientCertificates = new List<X509Certificate2>();
            List<X509Certificate2> certAuthorities = new List<X509Certificate2>();

            // gets all ClientSideCredential elements
            IEnumerable<XElement> clientCredElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientSideCredential");
            IEnumerable<XElement> certElements = null; // list of client certificate elements
            IEnumerable<XElement> caElements = null; // list of CA elements

            // gets client certificates and adds them to client certificate list
            foreach (XElement el in clientCredElements)
            {
                // checks every ClientSideCredential element for ClientCertificate elements
                certElements = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientCertificate");
                if (certElements.Any())
                {
                    base64Client = certElements.First().Value;
                    if (base64Client != "") // checks that the certificate value is not empty
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
                    else
                    {
                        // MessageBox.Show("Client certificate is empty.");
                    }
                }
                else
                {
                    // MessageBox.Show("No client certificates found.");
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
                    // adds certificate object to list
                    certAuthorities.Add(caCert);
                }
            }

            // gets client certificates and CAs and adds them to their respective lists
            //foreach (XElement el in authMethodElements)
            //{
                
                //// AuthenticationMethod element only has one ClientCertificate element, so gets first
                //base64Client = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "ClientCertificate").FirstOrDefault().Value;
                //if (base64Client != "") // excludes if empty
                //{
                //    // gets passphrase element
                //    clientPwd = el.DescendantsAndSelf().Elements().Where(pw => pw.Name.LocalName == "Passphrase").FirstOrDefault().Value;
                //    // converts from base64
                //    clientBytes = Convert.FromBase64String(base64Client);
                //    // creates certificate object
                //    clientCert = new X509Certificate2(clientBytes, clientPwd, X509KeyStorageFlags.PersistKeySet);
                //    // adds certificate object to list
                //    clientCertificates.Add(clientCert);
                //}

            //    // AuthenticationMethod element can have multiple CA elements, so loops through them
            //    caElements = el.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "CA");
            //    foreach (XElement caElement in caElements)
            //    {
            //        base64Ca = caElement.Value;
            //        if (base64Ca != "") // excludes if empty
            //        {
            //            // converts from base64
            //            caBytes = Convert.FromBase64String(base64Ca);
            //            // creates certificate object
            //            ca = new X509Certificate2(caBytes);
            //            // adds certificate object to list
            //            certAuthorities.Add(ca);
            //        }
            //    }
            //}
            // returns both certificate lists
            return Tuple.Create(clientCertificates, certAuthorities);
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

        // exits the application
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //List<string> newList = new List<string>();
            //newList.Add("boyoboy");
            //newList.Add("isthisworking");

            //string mahXml = ProfileXml.CreateProfileXml("yesLord", ProfileXml.EapType.PEAP_MSCHAPv2, newList);

            UserDataXml.CreateUserDataXml("myname", "ispassword");
        }
                
    }
}
