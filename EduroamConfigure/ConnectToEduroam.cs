using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManagedNativeWifi;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Contains various functions for:
    /// - installing certificates
    /// - creating a wireless profile
    /// - setting user data
    /// - connecting to a network
    /// </summary>
    public class ConnectToEduroam
    {
        // TODO: move these static variables to the caller

        // EAP type of selected configuration
        // client certificate valid from
        public static DateTime CertValidFrom { get; set; } // TODO: use EapAuthMethodInstaller.CertValidFrom instead

        // Configuration for how to install the eduroam profile to Windows:
        
        // Certificate stores
        private const StoreName caStoreName = StoreName.Root; // Used to install CAs to verify server certificates with
        private const StoreLocation caStoreLocation = StoreLocation.CurrentUser; // TODO: make this configurable to LocalMachine
        private const StoreName userCertStoreName = StoreName.My; // Used to install TLS client certificates
        private const StoreLocation userCertStoreLocation = StoreLocation.CurrentUser;

        // Profile.xml - EAP auth mode, ssid, allowed CA fingerprint, and failure modes
        // See 'dwFlags' at: https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
        private const ProfileType profileType = ProfileType.AllUser; // TODO: make this work as PerUser

        // UserData.xml - EAP user credentials
        // See 'dwFlags' at: https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofileeapxmluserdata
        private const uint profileUserType = 0x00000000; // "current user" - https://github.com/rozmansi/WLANSetEAPUserData
        //private const uint profileUserType = 0x00000001; // WLAN_SET_EAPHOST_DATA_ALL_USERS


        /// <summary>
        /// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
        /// Refer to frmSummary.InstallEapConfig to see how to use it (TODO: actually explain when finalized)
        /// </summary>
        /// <param name="eapConfig">EapConfig object</param>
        /// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
        public static IEnumerable<EapAuthMethodInstaller> InstallEapConfig(EapConfig eapConfig)
        {
            List<EduroamNetwork> eduroamNetworks = EduroamNetwork.EnumerateEduroamNetworks().ToList();
            if (!eduroamNetworks.Any())
                yield break; // TODO: concider throwing
            if (!EapTypeIsSupported(eapConfig))
                yield break; // TODO: concider throwing

            foreach (EapConfig.AuthenticationMethod authMethod in eapConfig.AuthenticationMethods)
            {
                if (EapTypeIsSupported(authMethod.EapType))
                    yield return new EapAuthMethodInstaller(authMethod, eduroamNetworks);
                // if EAP type is not supported, skip this authMethod
            }
        }

        /// <summary>
        /// Checks if eapConfig contains any supported authentification methods.
        /// If no such method exists, then warn the user before trying to install the config.
        /// </summary>
        /// <param name="eapConfig">The EAP config to check</param>
        /// <returns>True if it contains a supported type</returns>
        public static bool EapTypeIsSupported(EapConfig eapConfig)
        {
            foreach (EapConfig.AuthenticationMethod authMethod in eapConfig.AuthenticationMethods)
                if (EapTypeIsSupported(authMethod.EapType))
                    return true;
            return false;
        }

        private static bool EapTypeIsSupported(EapType eapType)
        {
            switch (eapType)
            {
                // Supported EAP types:
                case EapType.TLS:
                case EapType.TTLS: // not fully there yet
                    // TODO: Since this profile supports TTLS, be sure that any error returned is about TTLS not being supported
                case EapType.PEAP:
                    return true;
                default:
                    return false;
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
            private readonly List<EduroamNetwork> EduroamNetworks;
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
            public EapAuthMethodInstaller(EapConfig.AuthenticationMethod authMethod, List<EduroamNetwork> eduroamNetworks)
            {
                AuthMethod = authMethod;
                EduroamNetworks = eduroamNetworks;
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
                    var clientCert = new X509Certificate2(clientCertBytes, AuthMethod.ClientCertificatePassphrase, X509KeyStorageFlags.PersistKeySet);

                    // sets friendly name of certificate
                    clientCert.FriendlyName = clientCert.GetNameInfo(X509NameType.SimpleName, false);

                    // open personal certificate store to add client cert
                    using var personalStore = new X509Store(userCertStoreName, userCertStoreLocation);
                    personalStore.Open(OpenFlags.ReadWrite);
                    personalStore.Add(clientCert); // TODO: does this fail if done multiple times? perhaps add a guard like for CAs
                    personalStore.Close();

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
                using var rootStore = new X509Store(caStoreName, caStoreLocation);
                rootStore.Open(OpenFlags.ReadWrite);
                //foreach (string ca in AuthMethod.CertificateAuthorities)
                foreach (var caCert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
                {
                    if (caCert.NotAfter < DateTime.Now)
                    {
                        throw new EduroamAppUserError("expired CA",
                            "One of the provided Certificate Authorities from this institution has expired!\r\n" +
                            "Please contact the institution to have the issue fixed.");
                    }

                    // check if CA is not already installed
                    X509Certificate2Collection matchingCerts = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, true);
                    if (matchingCerts.Count < 1)
                    {
                        return true; // user must be informed
                    }
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
                using var rootStore = new X509Store(caStoreName, caStoreLocation);
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
                            // ^ Will produce a popup if the certificate is not already installed
                        }
                        catch (CryptographicException ex)
                        {
                            // if user selects No when prompted to install the CA
                            if ((uint)ex.HResult == 0x800704C7)
                                return false;

                            // unknown exception
                            throw;
                        }
                    }

                    // get CA thumbprint and adds to list
                    CertificateThumbprints.Add(caCert.Thumbprint);
                }

                string clientCertIssuer = InstallClientCertificate();

                // get thumbprints of already installed CAs that match client certificate issuer
                if (clientCertIssuer != null)
                {
                    // get CAs by client certificate issuer name
                    X509Certificate2Collection existingCAs = rootStore.Certificates
                        .Find(X509FindType.FindByIssuerDistinguishedName, clientCertIssuer, true);

                    // get CA thumbprint and adds to list
                    foreach (X509Certificate2 ca in existingCAs)
                    {
                        CertificateThumbprints.Add(ca.Thumbprint);
                    }
                }
                HasInstalledCertificates = true;
                return true;
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
                string serverNames = string.Join(";", AuthMethod.ServerNames);

                // generate new profile xml
                var profileXml = ProfileXml.CreateProfileXml(
                    EduroamNetwork.Ssid,
                    AuthMethod.EapType,
                    serverNames,
                    CertificateThumbprints);

                // create a new wireless profile
                bool any_installed = false;
                foreach (EduroamNetwork eduroamInstance in EduroamNetworks)
                {
                    any_installed |= CreateNewProfile(eduroamInstance.InterfaceId, profileXml);
                    // TODO: update docstring and handling in frmSummary due to any_installed
                }

                // check if EAP type is TLS and there is no client certificate
                if (AuthMethod.EapType == EapType.TLS && string.IsNullOrEmpty(AuthMethod.ClientCertificate))
                    return false;

                return any_installed;
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
            // security type not required
            const string securityType = null; // TODO: document why

            // overwrites if profile already exists
            const bool overwrite = true;

            return NativeWifi.SetProfile(interfaceId, profileType, profileXml, securityType, overwrite);
        }

        /// <summary>
        /// Deletes all network profile matching ssid, which is "eduroam" by default
        /// </summary>
        /// <param name="ssid">ssid to delete all profiles of</param>
        /// <returns>True if any profile deletion was succesful</returns>
        public static bool RemoveAllProfiles(string ssid = EduroamNetwork.Ssid)
        {
            bool ret = false;
            foreach (Guid interfaceId in EduroamNetwork.GetAllInterfaceIds())
            {
                if (RemoveProfile(interfaceId, ssid))
                    ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Deletes a network profile by matching ssid on specified network interface
        /// </summary>
        /// <returns>True if profile delete was succesful</returns>
        private static bool RemoveProfile(Guid interfaceId, string ssid = EduroamNetwork.Ssid)
        {
            return NativeWifi.DeleteProfile(interfaceId, ssid);
        }

        /// <summary>
        /// Creates user data xml for connecting using credentials.
        /// </summary>
        /// <param name="username">User's username optionally with realm</param>
        /// <param name="password">User's password.</param>
        /// <param name="eapType">EapType of installed profike</param>
        public static void SetupLogin(string username, string password, EapType eapType)
        {
            // TODO: move into EapAuthMethodInstaller

            // generates user data xml file
            string userDataXml = UserDataXml.CreateUserDataXml(username, password, eapType);

            // sets user data
            foreach (EduroamNetwork network in EduroamNetwork.EnumerateEduroamNetworks())
            {
                SetUserData(network.InterfaceId, EduroamNetwork.Ssid, userDataXml);
                // TODO: use return value
            }
        }

        /// <summary>
        /// Sets user data for a wireless profile.
        /// </summary>
        /// <param name="interfaceId">Interface ID of selected network.</param>
        /// <param name="profileName">Name of associated wireless profile.</param>
        /// <param name="userDataXml">User data XML converted to string.</param>
        /// <returns>True if succeeded, false if failed.</returns>
        private static bool SetUserData(Guid interfaceId, string profileName, string userDataXml)
        {
            return NativeWifi.SetProfileUserData(interfaceId, profileName, profileUserType, userDataXml);
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
        /// Connects to any eduroam wireless LAN
        /// </summary>
        /// <returns>True if successfully connected. False if not.</returns>
        private static async Task<bool> ConnectAsync()
        {
            // gets updated eduroam network packs
            foreach (AvailableNetworkPack network in EduroamNetwork.GetAllEduroamPacks())
            {
                if (string.IsNullOrEmpty(network.ProfileName)) continue; // TODO: will cause NativeWifi to throw, but should not happen

                // TODO: do in parallel instead of sequentially?

                // attempt to connect
                bool success = await NativeWifi.ConnectNetworkAsync(
                    interfaceId: network.Interface.Id,
                    profileName: network.ProfileName,
                    bssType: network.BssType,
                    timeout: TimeSpan.FromSeconds(5));
                if (success)
                    return true;
            }
            return false;
        }

    }

}
