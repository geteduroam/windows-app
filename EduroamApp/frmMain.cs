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
            // sets eduroam as chosen network
            AvailableNetworkPack network = SetChosenNetwork();

            txtOutput.Text = $"Now connecting to {network.Ssid.ToString()}.";
        }

        private void btnSelectProfile_Click(object sender, EventArgs e)
        {
            string filePath = GetProfileXml();
            string fileName = Path.GetFileName(filePath);
            txtProfilePath.Text = fileName;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string filePath = GetProfileXml();
            string fileName = Path.GetFileName(filePath);
            txtProfilePath.Text = fileName;

            // downloads and installs client certificate
            string certPassword = txtCertPwd.Text;
            var certificateResult = InstallCertificate(certPassword);
            txtOutput.Text += (certificateResult.Item1 ? "Certificate installed successfully.\n" : "Certificate installation failed.\n");
            txtOutput.Text += (certificateResult.Item2 ? "CA installed successfully.\n" : "CA installation failed.\n");
            string caThumbprint = certificateResult.Item3;

            // sets eduroam as chosen network
            AvailableNetworkPack network = SetChosenNetwork();
            string ssid = network.Ssid.ToString();
            Guid interfaceID = network.Interface.Id;

            // configures profile xml
            XElement newXml = ConfigureXml(filePath, ssid, caThumbprint);

            // creates a new network profile
            string xmlPath = txtProfilePath.Text;
            txtOutput.Text += (CreateNewProfile(interfaceID, newXml) ? "New profile successfully created.\n" : "Creation of new profile failed.\n");

            
            // connects to eduroam
            AvailableNetworkPack updatedNetwork = SetChosenNetwork();

            var connectResult = Task.Run(() => ConnectAsync(updatedNetwork)).Result;            
            txtOutput.Text += (connectResult ? "You are now connected to " + updatedNetwork.Ssid.ToString() + ".\n" : "Connection failed.\n");
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
                    if (network.ProfileName != "")
                    {
                        return network;
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
        public string GetProfileXml()
        {
            string xmlPath = "not found";

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

        public XElement ConfigureXml(string xmlFile, string ssid, string thumb)
        {
            XElement doc = XElement.Load(xmlFile);
            XNamespace ns = "http://www.microsoft.com/networking/WLAN/profile/v1";
            XNamespace ns2 = "http://www.microsoft.com/networking/OneX/v1";
            XNamespace ns3 = "http://www.microsoft.com/provisioning/EapHostConfig";
            XNamespace ns4 = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
            XNamespace ns5 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";


            XElement profileName = doc.Element(ns + "name");

            XElement ssidName = doc.Element(ns + "SSIDConfig")
                                    .Element(ns + "SSID")
                                    .Element(ns + "name");

            XElement thumbprint = doc.Element(ns + "MSM")
                                     .Element(ns + "security")
                                     .Element(ns2 + "OneX")
                                     .Element(ns2 + "EAPConfig")
                                     .Element(ns3 + "EapHostConfig")
                                     .Element(ns3 + "Config")
                                     .Element(ns4 + "Eap")
                                     .Element(ns5 + "EapType")
                                     .Element(ns5 + "ServerValidation")
                                     .Element(ns5 + "TrustedRootCA");

            profileName.Value = ssid;
            ssidName.Value = ssid;
            thumbprint.Value = thumb;

            return doc;
        }

        /// <summary>
        /// Creates new network profile according to selected network and profile XML.
        /// </summary>
        /// <param name="networkPack">Info about selected network.</param>
        /// <param name="profileXml">Path of selected profile XML.</param>
        /// <returns>True if succeeded, false if failed.</returns>
        public static bool CreateNewProfile(Guid networkID, XElement profileXml)
        {                      
            // sets the profile type to be Per User (value = 2)
            // if set to Per User, the security type parameter is not required
            ProfileType newProfileType = ProfileType.PerUser;

            // gets the content of the XML file
            string xmlContent = profileXml.ToString();

            // security type not required
            string newSecurityType = null;

            // overwrites if profile already exists
            bool overwrite = true;

            return NativeWifi.SetProfile(networkID, newProfileType, xmlContent, newSecurityType, overwrite);
        }

        /// <summary>        
        /// Downloads and installs a client certificate as well as a Client Authority certificate.
        /// </summary>
        /// <param name="password">The certificate's password.</param>        
        /// <returns>Certificate install success, CA install success and CA thumbprint.</returns>
        public Tuple<bool, bool, string> InstallCertificate(string password)
        {
            bool certSuccess = true;
            bool caSuccess = true;
            string caThumbprint = null;

            // creates directory to download certificate file to
            string path = $@"{ Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) }\MyCertificates";
            Directory.CreateDirectory(path);
            // sets the file name
            string certFile = "eduroam_certificate.p12";
            string caFile = "Fyrkat+Root+CA.crt";

            // downloads files from server
            using (WebClient client = new WebClient())
            {
                //client.DownloadFile("https://nofile.io/g/JFVD0GuZIlAN75eOSBWPf70HW7ttzarSoI6ToMvqXDNOgdQa1LBIyg1y2inkTlVZ/ericv%40fyrkat.no.p12/", $@"{path}\{certFile}");                
                //client.DownloadFile("https://nofile.io/g/Y42wuToV5J2Cz1lyUdwaAcdrW6LIT6v228YduZFRRhb92sMfQ6zLZK828gHygotI/Fyrkat%2BRoot%2BCA.crt/", $@"{path}\{caFile}");                
            }

            // installs certficate to personal certificate store
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2 certificate = new X509Certificate2($@"{path}\{certFile}", password, X509KeyStorageFlags.PersistKeySet); // include PersistKeySet flag so certificate is valid after reboot
                store.Add(certificate);                
                store.Close(); // closes the certificate store
            }
            catch (Exception) {
                certSuccess = false;
            }

            // installs CA to trusted root certificate authority store
            try
            {
                X509Store caStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                X509Certificate2 ca = new X509Certificate2($@"{path}\{caFile}");
                caStore.Open(OpenFlags.ReadWrite);
                // show messagebox to let users know about the CA installation warning
                MessageBox.Show("You will now be prompted to install the Certificate Authority. In order to connect to eduroam, you need to accept this by pressing \"Yes\".","Accept Certificate Authority", MessageBoxButtons.OK);
                caStore.Add(ca);
                caThumbprint = ca.Thumbprint; // gets thumbprint of CA
                caStore.Close(); // closes the certificate store
            }
            catch (Exception) {
                caSuccess = false;
            }

            return Tuple.Create(certSuccess, caSuccess, caThumbprint); // returns two boolean values to confirm success
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


    }
}
