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


        /// <summary>
        /// Checks the EAP config to see if there is any issues
        /// TODO: test this
        /// </summary>
        /// <returns>A tuple on the form: (bool isCritical, string description)</returns>
        public static IEnumerable<ValueTuple<bool, string>> LookForWarningsInEapConfig(EapConfig eapConfig)
        {
            if (!EapConfigIsSupported(eapConfig))
            {
                yield return (true, "This configuration is not supported");
                yield break;
            }

            if (!eapConfig.AuthenticationMethods
                    .Where(AuthMethodIsSupported)
                    .All(authMethod => authMethod.CertificateAuthorities.Any()))
                yield return (true, "This configuration is missing Certificate Authorities");


            DateTime now = DateTime.Now;
            bool has_expired_ca = eapConfig.AuthenticationMethods
                .Where(AuthMethodIsSupported)
                .SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
                .Any(caCert => caCert.NotAfter < now);

            bool has_valid_ca = eapConfig.AuthenticationMethods
                .Where(AuthMethodIsSupported)
                .SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
                .Where(caCert => now < caCert.NotAfter)
                .Any(caCert => caCert.NotBefore < now);

            if (has_expired_ca)
            {
                yield return has_valid_ca switch
                {
                    true => (false,
                        "One of the provided Certificate Authorities from this institution has expired.\r\n" +
                        "There might be some issues connecting to eduroam."),
                    false => (true, // TODO: This case means that the ProfileXml will be configured to not expect any fingerprint, meaning no connection can be made
                        "The provided Certificate Authorities from this institution have all expired!\r\n" +
                        "Please contact the institution to have the issue fixed!"),
                };
            }
            else if (!has_valid_ca)
            {
                DateTime earliest = eapConfig.AuthenticationMethods
                    .Where(AuthMethodIsSupported)
                    .SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
                    .Where(caCert => now < caCert.NotAfter)
                    .Max(caCert => caCert.NotBefore);

                yield return (false,
                    "The Certificate Authorities in this configuration has yet to become valid.\r\n" +
                    "This configuration will become valid in " + (earliest - now).TotalMinutes + " minutes.");
            }
        }

        /// <summary>
        /// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
        /// Refer to frmSummary.InstallEapConfig to see how to use it (TODO: actually explain when finalized)
        /// </summary>
        /// <param name="eapConfig">EapConfig object</param>
        /// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
        public static IEnumerable<EapAuthMethodInstaller> InstallEapConfig(EapConfig eapConfig)
        {
            List<EduroamNetwork> eduroamNetworks = EduroamNetwork.GetAll().ToList();
            if (!eduroamNetworks.Any())
                yield break; // TODO: concider throwing
            if (!EapConfigIsSupported(eapConfig))
                yield break; // TODO: concider throwing

            foreach (EapConfig.AuthenticationMethod authMethod in eapConfig.AuthenticationMethods)
            {
                if (AuthMethodIsSupported(authMethod))
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
        public static bool EapConfigIsSupported(EapConfig eapConfig)
        {
            return eapConfig.AuthenticationMethods
                .Where(AuthMethodIsSupported).Any();
        }

        private static bool AuthMethodIsSupported(EapConfig.AuthenticationMethod authMethod)
        {
            return ProfileXml.IsSupported(authMethod)
                && UserDataXml.IsSupported(authMethod);
        }

        /// <summary>
        /// A class which helps you install one of the authMethods
        /// in a EapConfig, designed to be interactive wiht the user.
        /// </summary>
        public class EapAuthMethodInstaller
        {
            // all the CA thumbprints that will be added to Wireless Profile XML
            private readonly List<string> CertificateThumbprints = new List<string>();
            private readonly List<EduroamNetwork> EduroamNetworks;

            // To track proper order of operations
            private bool HasInstalledCertificates = false; 
            private bool HasInstalledProfile = false;

            public readonly EapConfig.AuthenticationMethod AuthMethod;
            public DateTime CertValidFrom { get; private set; }


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
                    var clientCert = AuthMethod.ClientCertificateAsX509Certificate2();

                    // open personal certificate store to add client cert
                    using var personalStore = new X509Store(userCertStoreName, userCertStoreLocation);
                    personalStore.Open(OpenFlags.ReadWrite);
                    personalStore.Add(clientCert);
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
            public bool NeedsToInstallCAs()
            {
                using var rootStore = new X509Store(caStoreName, caStoreLocation);
                rootStore.Open(OpenFlags.ReadOnly);
                //foreach (string ca in AuthMethod.CertificateAuthorities)
                foreach (var caCert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
                {
                    // check if CA is not already installed
                    X509Certificate2Collection matchingCerts = rootStore.Certificates.Find(
                        X509FindType.FindByThumbprint, caCert.Thumbprint, false);
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
                    X509Certificate2Collection matchingCerts = rootStore.Certificates.Find(X509FindType.FindByThumbprint, caCert.Thumbprint, false);
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

                // TODO: dedup CertificateThumbprints

                return true;
            }

            /// <summary>
            /// Will install the authMethod as a profile
            /// Having run InstallCertificates successfully before calling this is a prerequisite
            /// If this returns FALSE: It means there is a missing TLS client certificate left to be installed
            /// </summary>
            /// <returns>True if the profile was installed on any interface</returns>
            public bool InstallProfile()
            {
                if (!HasInstalledCertificates)
                    throw new EduroamAppUserError("missing certificates",
                        "You must first install certificates with InstallCertificates");

                // generate new profile xml
                var profileXml = ProfileXml.CreateProfileXml(
                    AuthMethod,
                    CertificateThumbprints);

                // create a new wireless profile
                bool any_installed = false;
                foreach (EduroamNetwork network in EduroamNetworks)
                {
                    any_installed |= network.InstallProfile(profileXml);
                }

                if (AuthMethod.EapType == EapType.TLS) // TODO: this is hacky, SetupLogin should be a part of InstallProfile
                { 
                    SetupLogin(null, null, AuthMethod);
                }

                HasInstalledProfile = any_installed;
                return any_installed;
            }

            public bool NeedsLoginCredentials()
            {
                if (!HasInstalledProfile)
                    throw new EduroamAppUserError("profile not installed",
                        "You must first install the profile with InstallProfile");
                return AuthMethod.NeedsLoginCredentials();
            }

            public bool NeedsClientCertificate()
            {
                if (!HasInstalledProfile)
                    throw new EduroamAppUserError("profile not installed",
                        "You must first install the profile with InstallProfile");
                return AuthMethod.NeedsClientCertificate();
            }
        }


        /// <summary>
        /// Deletes all network profile matching ssid, which is "eduroam" by default
        /// </summary>
        /// <returns>True if any profile deletion was succesful</returns>
        public static bool RemoveAllProfiles()
        {
            bool ret = false;
            foreach (EduroamNetwork network in EduroamNetwork.GetAll())
            {
                if (network.RemoveProfile())
                    ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Creates and installs user data xml into all network interfaces
        /// </summary>
        /// <param name="username">User's username optionally with realm</param>
        /// <param name="password">User's password.</param>
        /// <param name="authMethod">AuthMethod of installed profile</param>
        public static bool SetupLogin(string username, string password, EapConfig.AuthenticationMethod authMethod)
        {
            // TODO: move into EapAuthMethodInstaller?

            // generates user data xml file
            string userDataXml = UserDataXml.CreateUserDataXml(
                authMethod,
                username,
                password);

            // sets user data
            bool anyInstalled = false;
            foreach (EduroamNetwork network in EduroamNetwork.GetAll())
            {
                anyInstalled |= network.InstallUserData(userDataXml);
            }
            return anyInstalled;
        }

        /// <summary>
        /// Attempts to connects to any eduroam wireless LAN, in succession
        /// </summary>
        /// <returns>True if successfully connected. False if not.</returns>
        public static async Task<bool> TryToConnect()
        {
            // gets updated eduroam network packs
            foreach (var network in EduroamNetwork.GetAll())
            {
                // TODO: do in parallel instead of sequentially?

                bool success = await network.TryToConnect();
                if (success)
                    return true;
            }
            return false;
        }

    }

}
