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

namespace EduroamApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSelectProfile_Click(object sender, EventArgs e)
        {
            string rawXmlPath = GetProfileXml();
            txtProfilePath.Text = rawXmlPath;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            AvailableNetworkPack network = SetChosenNetwork();
            string xmlPath = txtProfilePath.Text;
            // creates a new network profile
            txtOutput.Text += (CreateNewProfile(network, xmlPath) ? "New profile successfully created." : "Creation of new profile failed.");

            // downloads and installs client certificate and CA
            string certPassword = txtCertPwd.Text;
            InstallCertificate(certPassword);
                        
            // connects to eduroam

        }




        // -------------------------------------------- FUNCIONS ------------------------------------------------------------

        /// <summary>
        /// Sets Eduroam as the chosen network to connect to. Exits the application if Eduroam is not available.
        /// </summary>
        public static AvailableNetworkPack SetChosenNetwork()
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
        public static string GetProfileXml()
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

            string xmlFile = File.ReadAllText(xmlPath);
            return xmlPath;
        }

        /// <summary>
        /// Creates new network profile according to selected network and profile XML.
        /// </summary>
        /// <param name="networkPack">Info about selected network.</param>
        /// <param name="profileXml">Path of selected profile XML.</param>
        /// <returns>True if succeeded, false if failed.</returns>
        public static bool CreateNewProfile(AvailableNetworkPack networkPack, string profileXml)
        {                      
            // sets the profile type to be Per User (value = 2)
            // if set to Per User, the security type parameter is not required
            ProfileType newProfileType = ProfileType.PerUser;

            // gets the content of the XML file
            string xmlContent = File.ReadAllText(profileXml);

            // security type not required
            string newSecurityType = null;

            // overwrites if profile already exists
            bool overwrite = true;

            return NativeWifi.SetProfile(networkPack.Interface.Id, newProfileType, xmlContent, newSecurityType, overwrite);
        }

        /// <summary>
        /// Downloads and installs a client certificate as well as a Client Authority certificate.
        /// </summary>
        /// <param name="password">The certificate's password.</param>
        public static void InstallCertificate(string password)
        {
            // checks if valid certificate already exists
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "eduroam_certificate.p12", true);
            if (certs.Count < 1)
            {
                // creates directory to download certificate file to
                string path = $@"{ Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) }\MyCertificates";
                Directory.CreateDirectory(path);
                // sets the file name
                string file = "eduroam_certificate.p12";
                // downloads certificate from server
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://nofile.io/g/HeaP3RqE5P0HVgrmGSzzA2jHWeLQbfw2a40T0vXwWKGEt38oLZ3pUEGzrPYegha3/ericv%40fyrkat.no.p12", $@"{path}\{file}");
                    Console.WriteLine($@"Certificate downloaded to {path}\{file}.");
                }

                // installs certficate to personal certificate store
                X509Certificate2 certificate = new X509Certificate2($@"{path}\{file}", password, X509KeyStorageFlags.PersistKeySet); // include PersistKeySet flag so certificate is valid after reboot
                store.Add(certificate);

                //Console.WriteLine("Certificate added to: " + store.Name);
            }
            // closes the certificate store
            store.Close();

            // adds certificate authority to trusted root certificate authority store
            X509Store caStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            X509Certificate2 ca = new X509Certificate2(@"C:\Users\lwerivel18\Documents\Certificates\Fyrkat+Root+CA.crt");
            caStore.Open(OpenFlags.ReadWrite);
            caStore.Add(ca);
            store.Close();
            //Console.WriteLine("Certificate Authority added to: " + caStore.Name);
        }


        /// <summary>
		/// Connects to the chosen wireless LAN
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
                timeout: TimeSpan.FromSeconds(100));
        }

        
    }
}
