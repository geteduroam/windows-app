using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManagedNativeWifi;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace EduroamApp
{
    /// <summary>
    /// Contains various functions for:
    /// - installing certificates
    /// - creating a wireless profile
    /// - setting user data
    /// - connecting to a network
    /// </summary>
    class ConnectToEduroam
    {
        // TODO: move these static variables to the caller

        // SSID of eduroam network
        private static string Ssid { get; set; }
        // Id of wireless network interface
        private static Guid InterfaceId { get; set; }
        // EAP type of selected configuration
        private static EapType EapType { get; set; }
        // client certificate valid from
        public static DateTime CertValidFrom { get; set; } // TODO: use EapAuthMethodInstaller.CertValidFrom instead


        /// <summary>
        /// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
        /// Refer to frmSummary.InstallEapConfig to see how to use it
        /// </summary>
        /// <param name="eapConfig">EapConfig object</param>
        /// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
        public static IEnumerable<EapAuthMethodInstaller> InstallEapConfig(EapConfig eapConfig)
        {
            // create new instance of eduroam network
            var eduroamInstance = new EduroamNetwork();
            // get SSID
            Ssid = eduroamInstance.Ssid;
            // get interface ID
            InterfaceId = eduroamInstance.InterfaceId;

            foreach (EapConfig.AuthenticationMethod authMethod in eapConfig.AuthenticationMethods)
            {
                switch (authMethod.EapType)
                {
                    // Supported EAP types:
                    case EduroamApp.EapType.TLS:
                    case EduroamApp.EapType.TTLS: // not fully there yet
                    case EduroamApp.EapType.PEAP:
                        yield return new EapAuthMethodInstaller(authMethod);
                        break;

                    // Since this profile supports TTLS, be sure that any error returned is about TTLS not being supported
                    default:
                        continue; // if EAP type is not supported, skip this authMethod
                }
            }
        }

        /// <summary>
        /// A class which helps you install one of the authMethods
        /// in a EapConfig, designed to be interactive wiht the user.
        /// </summary>
        public class EapAuthMethodInstaller
        {
            // all CA thumbprints that will be added to Wireless Profile XML
            private readonly List<string> CertificateThumbprints = new List<string>();
            private readonly EapConfig.AuthenticationMethod AuthMethod;
            private bool HasInstalledCertificates = false; // To track proper order of operations

            public DateTime CertValidFrom { get; private set; }

            public EapType EapType
            {
                get { return AuthMethod.EapType; }
            }

            /// <summary>
            /// Constructs a EapAuthMethodInstaller
            /// </summary>
            /// <param name="authMethod">The authentification method to attempt to install</param>
            public EapAuthMethodInstaller(EapConfig.AuthenticationMethod authMethod)
            {
                AuthMethod = authMethod;
            }

            /// <summary>
            /// Installs the client certificate into the personal
            /// certificate store of the windows current user
            /// </summary>
            /// <returns>
            /// Returns the name of the issuer of this client certificate,
            /// if there is any client certificate to install
            /// </returns>
            private string InstallClientCertificate()
            {
                // checks if Authentication method contains a client certificate
                if (!string.IsNullOrEmpty(AuthMethod.ClientCertificate))
                {
                    // creates certificate object from base64 encoded cert
                    var clientCertBytes = Convert.FromBase64String(AuthMethod.ClientCertificate);
                    var clientCert = new X509Certificate2(clientCertBytes, AuthMethod.ClientPassphrase, X509KeyStorageFlags.PersistKeySet);

                    // sets friendly name of certificate
                    clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);

                    // open personal certificate store to add client cert
                    var personalStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    try
                    {
                        personalStore.Open(OpenFlags.ReadWrite);
                        personalStore.Add(clientCert); // TODO: does this fail if done multiple times? perhaps add a guard like for CAs
                    }
                    finally
                    {
                        personalStore.Close();
                    }

                    // gets name of CA that issued the certificate
                    // gets valid from time of certificate
                    CertValidFrom = clientCert.NotBefore; // TODO: make gui use this
                    ConnectToEduroam.CertValidFrom = clientCert.NotBefore; // TODO: REMOVE

                    return clientCert.IssuerName.Name;
                }
                return null;
            }

            /// <summary>
            /// Call this to check if there are any CAs left to install
            /// </summary>
            /// <returns></returns>
            public bool NeedToInstallCAs()
            {
                var rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                rootStore.Open(OpenFlags.ReadWrite);

                //foreach (string ca in AuthMethod.CertificateAuthorities)
                foreach (var caCert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
                {
                    // check if CA is not already installed
                    X509Certificate2Collection matchingCerts = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, true);
                    if (matchingCerts.Count < 1)
                        return true; // user must be informed
                }
                return false;

            }

            /// <summary>
            /// Will install CAs and user certificates provided by the authMethod.
            /// Installing a CA in windows will produce a dialog box which the user must accept.
            /// This will quit partway through if the user refuses to install any CA, but it is safe to run again.
            /// Use NeedToInstallCAs to predict if it will need to install any CAs
            /// </summary>
            /// <returns>Returns true if all certificates has been successfully installed</returns>
            public bool InstallCertificates()
            {
                CertificateThumbprints.Clear();

                // open the trusted root CA store
                var rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                try
                {
                    rootStore.Open(OpenFlags.ReadWrite);

                    // get all CAs from Authentication method
                    foreach (var caCert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
                    {
                        // check if CA is not already installed
                        X509Certificate2Collection matchingCerts = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, true);
                        if (matchingCerts.Count < 1)
                        {
                            try
                            {
                                // add CA to trusted root store
                                rootStore.Add(caCert);
                            }
                            catch (CryptographicException ex)
                            {
                                // if user selects No when prompted to install CA
                                if ((uint)ex.HResult == 0x800704C7)
                                    return false;
                                throw; // unknown exception
                            }
                        }

                        // get CA thumbprint and formats it
                        string formattedThumbprint = Regex.Replace(caCert.Thumbprint, ".{2}", "$0 ");
                        CertificateThumbprints.Add(formattedThumbprint); // add thumbprint to list
                    }

                    string clientCertIssuer = InstallClientCertificate();

                    // get thumbprints of already installed CAs that match client certificate issuer
                    if (clientCertIssuer != null)
                    {
                        // get CAs by client certificate issuer name
                        X509Certificate2Collection existingCAs = rootStore.Certificates
                            .Find(X509FindType.FindByIssuerDistinguishedName, clientCertIssuer, true);

                        foreach (X509Certificate2 ca in existingCAs)
                        {
                            // get CA thumbprint and formats it
                            string formattedThumbprint = Regex.Replace(ca.Thumbprint, ".{2}", "$0 ");
                            // add thumbprint to list
                            CertificateThumbprints.Add(formattedThumbprint);
                        }
                    }
                    HasInstalledCertificates = true;
                    return true;
                }
                finally
                {
                    rootStore.Close(); // close trusted root store
                }
            }

            /// <summary>
            /// Will install the authMethod as a profile
            /// Having run InstallCertificates successfully before calling this is a prerequisite
            /// If this returns FALSE: It means there is a missing TLS client certificate left to be installed
            /// </summary>
            /// <returns>True on success, False if missing a client certificate</returns>
            public bool InstallProfile()
            {
                if (!HasInstalledCertificates)
                    throw new EduroamAppUserError("missing certificates", "You must first install certificates with InstallCertificates");

                // get server names of authentication method and joins them into one single string
                string serverNames = string.Join(";", AuthMethod.ServerName);

                // generate new profile xml
                var profileXml = EduroamApp.ProfileXml.CreateProfileXml(
                    EduroamInstance.Ssid,
                    AuthMethod.EapType,
                    serverNames,
                    CertificateThumbprints);

                // create a new wireless profile
                CreateNewProfile(InterfaceId, profileXml); // TODO: static variables

                // check if EAP type is TLS and there is no client certificate
                if (AuthMethod.EapType == EapType.TLS && string.IsNullOrEmpty(AuthMethod.ClientCertificate))
                    return false;

                return true;
            }
        }


        /// <summary>
        /// Creates new network profile according to selected network and profile XML.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <param name="profileXml">Profile XML</param>
        /// <returns>True if profile create success, false if not.</returns>
        private static bool CreateNewProfile(Guid interfaceId, string profileXml)
        {
            // sets the profile type to be All-user (value = 0)
            const ProfileType profileType = ProfileType.AllUser;

            // security type not required
            const string securityType = null;

            // overwrites if profile already exists
            const bool overwrite = true;

            return NativeWifi.SetProfile(interfaceId, profileType, profileXml, securityType, overwrite);
        }

        /// <summary>
        /// Deletes eduroam profile.
        /// </summary>
        /// <returns>True if profile delete succesful, false if not.</returns>
        public static bool RemoveProfile()
        {
            return NativeWifi.DeleteProfile(InterfaceId, Ssid);
        }

        /// <summary>
        /// Creates user data xml for connecting using credentials.
        /// </summary>
        /// <param name="username">User's username.</param>
        /// <param name="password">User's password.</param>
        public static void SetupLogin(string username, string password)
        {
            // generates user data xml file
            string userDataXml = UserDataXml.CreateUserDataXml(username, password, EapType);
            // sets user data
            SetUserData(InterfaceId, Ssid, userDataXml);
        }

        /// <summary>
        /// Sets user data for a wireless profile.
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
        /// Waits for async connection to complete.
        /// </summary>
        /// <returns>Connection result.</returns>
        public static Task<bool> WaitForConnect()
        {
            // runs async method
            Task<bool> connectResult = Task.Run(ConnectAsync);
            return connectResult;
        }

        /// <summary>
        /// Connects to the chosen wireless LAN.
        /// </summary>
        /// <returns>True if successfully connected. False if not.</returns>
        private static async Task<bool> ConnectAsync()
        {
            // gets updated eduroam network pack
            AvailableNetworkPack network = EduroamNetwork.GetEduroamPack();

            if (network == null)
                return false;

            return await NativeWifi.ConnectNetworkAsync(
                interfaceId: network.Interface.Id,
                profileName: network.ProfileName,
                bssType: network.BssType,
                timeout: TimeSpan.FromSeconds(5));
        }

    }

}
