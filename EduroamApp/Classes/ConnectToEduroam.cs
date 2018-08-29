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
        // SSID of eduroam network
        private static string ssid;
        // Id of wireless network interface
        private static Guid interfaceId;
        // xml file for building wireless profile
        private static string profileXml;

        public static uint Setup(EapConfig eapConfig)
        {
            // creates new instance of eduroam network
            var eduroamInstance = new EduroamNetwork();
            // gets SSID
            ssid = eduroamInstance.Ssid;
            // gets interface ID
            interfaceId = eduroamInstance.InterfaceId;

            // gets the first/default authentication method of an EAP config file
            EapConfig.AuthenticationMethod authMethod = eapConfig.AuthenticationMethods.First();

            // gets EAP type of authentication method
            uint eapType = authMethod.EapType;

            // if EAP type is not supported, cancel setup
            if (eapType != 13 && eapType != 25 && eapType != 21) return eapType;
            
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

                // gets the CA that issued the certificate
                certIssuer = clientCert.IssuerName.Name;
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
                    MessageBox.Show("You will now be prompted to install a Certificate Authority. \n" +
                                    "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
                        "Accept Certificate Authority", MessageBoxButtons.OK);

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
                            // if user selects No when prompted to install CA, show error and ask to retry or cancel
                            if ((uint)ex.HResult == 0x800704C7)
                            {
                                DialogResult retryCa = MessageBox.Show("CA not installed. \nIn order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
                                                                       "Accept Certificate Authority", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                // if user selects cancel, stop looping
                                if (retryCa == DialogResult.Cancel)
                                {
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

            // also gets thumbprints of already installed CAs that match client certificate issuer 
            if (certIssuer != null)
            {
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
            profileXml = EduroamApp.ProfileXml.CreateProfileXml(ssid, eapType, serverNames, thumbprints);

            // creates a new wireless profile
            CreateNewProfile();

            // checks if EAP type is TLS and there is no client certificate
            if (eapType == 13 && string.IsNullOrEmpty(authMethod.ClientCertificate))
            {
                DialogResult dialogResult = MessageBox.Show(
                    "The selected profile requires a separate client certificate. Do you want to browse your local files for one?",
                    "Client certificate required", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return (uint) (dialogResult == DialogResult.Yes ? 500 : 0);
            }

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
            AvailableNetworkPack network = EduroamNetwork.GetEduroamPack(); 

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
                timeout: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Creates new network profile according to selected network and profile XML.
        /// </summary>
        /// <returns>True if succeeded, false if failed.</returns>
        public static void CreateNewProfile()
        {
            // sets the profile type to be All-user (value = 0)
            // if set to Per User, the security type parameter is not required
            const ProfileType profileType = ProfileType.AllUser;
            
            // security type not required
            const string securityType = null;

            // overwrites if profile already exists
            const bool overwrite = true;

            NativeWifi.SetProfile(interfaceId, profileType, profileXml, securityType, overwrite);
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
            const uint profileUserType = 0x00000001;

            return NativeWifi.SetProfileUserData(networkId, profileName, profileUserType, userDataXml);
        }


        /// <summary>
        /// Creates EapConfig object from EAP config file.
        /// </summary>
        /// <param name="eapFile">EAP config file as string.</param>
        /// <returns>EapConfig object.</returns>
        public static EapConfig GetEapConfig(string eapFile)
        {
            // loads the XML file from its file path
            XElement doc = XElement.Parse(eapFile);
            
            // instantiates new EapConfig object
            var eapConfig = new EapConfig();
            // creates new list of authentication methods
            List<EapConfig.AuthenticationMethod> authMethods = new List<EapConfig.AuthenticationMethod>();

            // gets all AuthenticationMethods elements
            IEnumerable<XElement> authMethodElements = doc.DescendantsAndSelf().Elements().Where(cl => cl.Name.LocalName == "AuthenticationMethod");
            foreach (XElement element in authMethodElements)
            {
                // gets EAP method type
                var eapType = (uint)element.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Type");

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
                authMethods.Add(new EapConfig.AuthenticationMethod(eapType, certAuths, serverNames, clientCert, passphrase));
            }
            // adds the authentication method objects to the EapConfig object
            eapConfig.AuthenticationMethods = authMethods;

            // gets provider's  display name
            var displayName = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "DisplayName");
            // gets logo element
            XElement logoElement = doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "ProviderLogo");
            // gets provider's logo as base64 encoded string from logo element
            var logo = (string) logoElement;
            // gets the file format of the logo
            var logoFormat = (string) logoElement?.Attribute("mime");
            // gets provider's email address
            var emailAddress = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "EmailAddress");
            // gets provider's web address
            var webAddress = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "WebAddress");
            // gets provider's phone number
            var phone = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "Phone");
            // gets provider's phone number
            var termsOfUse = (string)doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "TermsOfUse");
            // gets identity element
            XElement eapIdentityElement = doc.DescendantsAndSelf().Elements().FirstOrDefault(x => x.Name.LocalName == "EAPIdentityProvider");
            // gets institution ID from identity element
            var instId = (string)eapIdentityElement?.Attribute("ID");

            // adds the provider info to the EapConfig object
            eapConfig.InstitutionInfo = new EapConfig.ProviderInfo(displayName, logo, logoFormat, emailAddress, webAddress, phone, instId, termsOfUse);

            // returns the EapConfig object
            return eapConfig;
        }

        /// <summary>
        /// Converts base64 string to image.
        /// </summary>
        /// <param name="base64String">Base64 string.</param>
        /// <returns>Image.</returns>
        public static Image Base64ToImage(string base64String)
        {
            // convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }
    }
}
