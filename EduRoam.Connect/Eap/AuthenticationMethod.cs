using EduRoam.Connect.Exceptions;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect.Eap
{

    /// <summary>
    /// AuthenticationMethod contains information about client certificates and CAs.
    /// </summary>
    public class AuthenticationMethod
    {
        #region Properties

        public EapConfig? EapConfig { get; } // reference to parent EapConfig
        public EapType EapType { get; }
        public InnerAuthType InnerAuthType { get; }
        public List<string> ServerCertificateAuthorities { get; } // base64 encoded DER certificate
        public List<string> ServerNames { get; }
        public string? ClientUserName { get; } // preset inner identity, expect it to have a realm
        public string? ClientPassword { get; } // preset outer identity
        public string? ClientCertificate { get; } // base64 encoded PKCS12 certificate+privkey bundle
        public string? ClientCertificatePassphrase { get; } // passphrase for ^
        public string? ClientOuterIdentity { get; } // expect it to have a realm. Also known as: anonymous identity, routing identity
        public string? ClientInnerIdentitySuffix { get; } // realm
        public bool ClientInnerIdentityHint { get; } // Wether to disallow subrealms or not (see https://github.com/GEANT/CAT/issues/190)

        public bool IsHS20Supported { get => this.EapConfig?.CredentialApplicabilities.Any(cred => cred.ConsortiumOid != null) ?? false; }
        public bool IsSSIDSupported { get => this.EapConfig?.CredentialApplicabilities.Any(cred => cred.Ssid != null && cred.Ssid.Length != 0) ?? false; }

        #endregion Properties

        #region Helpers

        // TODO: Also add wired 802.1x support
        public List<string> SSIDs
        {
            get => this.EapConfig?.CredentialApplicabilities
                .Where(cred => cred.NetworkType == IEEE802x.IEEE80211)
                .Where(cred => cred.MinRsnProto != "TKIP") // Too old and insecure
                .Where(cred => cred.Ssid != null) // Filter out HS20 entries, those have no SSID
                .Select(cred => cred.Ssid!)
                .ToList() ?? new List<string>();
        }
        public List<string> ConsortiumOIDs
        {
            get => this.EapConfig?.CredentialApplicabilities
                .Where(cred => cred.ConsortiumOid != null)
                .Select(cred => cred.ConsortiumOid!)
                .ToList() ?? new List<string>();
        }
        public DateTime? ClientCertificateNotBefore
        {
            get
            {
                using var cert = this.ClientCertificateAsX509Certificate2();
                return cert?.NotBefore;
            }
        }
        public DateTime? ClientCertificateNotAfter
        {
            get
            {
                using var cert = this.ClientCertificateAsX509Certificate2();
                return cert?.NotAfter;
            }
        }

        private byte[] ClientCertificateRaw
        {
            get => Convert.FromBase64String(this.ClientCertificate ?? string.Empty);
        }

        private bool CertificateIsValid
        {
            get => VerifyCertificateBundle(
                    Convert.FromBase64String(this.ClientCertificate ?? string.Empty),
                    this.ClientCertificatePassphrase);
        }

        #endregion Helpers

        #region CertExport

        /// <summary>
        /// Converts and enumerates CertificateAuthorities as X509Certificate2 objects.
        /// </summary>
        /// <remarks>The certificates must be disposed after use</remarks>
        public IEnumerable<X509Certificate2> CertificateAuthoritiesAsX509Certificate2()
        {
            foreach (var ca in this.ServerCertificateAuthorities)
            {
                X509Certificate2 cert;
                try
                {
                    // TODO: find some nice way to ensure these are disposed of properly
                    cert = new X509Certificate2(Convert.FromBase64String(ca));
                }
                catch (CryptographicException)
                {
                    throw new EduroamAppUserException("corrupt certificate",
                        "EAP profile has an malformed or corrupted certificate");
                }

                // sets the friendly name of certificate
                if (string.IsNullOrEmpty(cert.FriendlyName))
                {
                    cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);
                }

                yield return cert;
            }
        }

        /// <summary>
        /// Converts the client certificate base64 data to a X509Certificate2 object
        /// </summary>
        /// <returns>X509Certificate2 if any. Null if non exist or the passphrase is incorrect</returns>
        public X509Certificate2? ClientCertificateAsX509Certificate2()
        {
            if (string.IsNullOrEmpty(this.ClientCertificate))
            {
                return null;
            }

            try
            {
                var cert = new X509Certificate2(
                    Convert.FromBase64String(this.ClientCertificate),
                    this.ClientCertificatePassphrase,
                    X509KeyStorageFlags.PersistKeySet);

                // sets the friendly name of certificate
                if (string.IsNullOrEmpty(cert.FriendlyName))
                {
                    cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
                }

                return cert;
            }
            catch (CryptographicException ex)
            {
                if ((ex.HResult & 0xFFFF) == 0x56)
                {
                    return null; // wrong passphrase
                }

                throw new EduroamAppUserException("corrupt client certificate",
                    "EAP profile has an malformed or corrupted client certificate");
            }
        }

        #endregion CertExport

        #region Verification

        // methods to check if the authentification method is complete, and methods to mend it

        /// <summary>
        /// If this returns true, then the user must provide the login credentials
        /// when installing with ConnectToEduroam or EduroamNetwork
        /// </summary>
        public bool NeedsLoginCredentials
        {
            get => this.EapType != EapType.TLS // If the auth method expects login credentials
                && (string.IsNullOrEmpty(this.ClientUserName) // but we don't already have them
                    || string.IsNullOrEmpty(this.ClientPassword)
                );
        }

        /// <summary>
        /// If this is true, then you must provide a
        /// certificate file and add it with this.AddClientCertificate
        /// </summary>
        public bool NeedsClientCertificate
        {
            get => this.EapType == EapType.TLS // If we use EAP-TLS
                && string.IsNullOrEmpty(this.ClientCertificate); // and we don't already have a certificate
        }

        /// <summary>
        /// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
        /// Add this passphrase with this.AddClientCertificatePassphrase
        /// </summary>
        public bool NeedsClientCertificatePassphrase
        {
            get => this.EapType == EapType.TLS // If we use EAP-TLS
                && !string.IsNullOrEmpty(this.ClientCertificate) // and we DO have a client certificate
                && !this.CertificateIsValid; // but we cannot read it yet
        }

        #endregion Verification

        #region Credentials
        /// <summary>
        /// Adds the username and password to be installed along with the wlan profile
        /// </summary>
        /// <param name="username">The username for inner auth</param>
        /// <param name="password">The passpword for inner auth</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        public AuthenticationMethod? WithLoginCredentials(string username, string password)
            => this.EapType == EapType.TLS
                ? null
                : new AuthenticationMethod(
                    this.EapType,
                    this.InnerAuthType,
                    this.ServerCertificateAuthorities,
                    this.ServerNames,
                    username,
                    password,
                    this.ClientCertificate,
                    this.ClientCertificatePassphrase,
                    this.ClientOuterIdentity,
                    this.ClientInnerIdentitySuffix,
                    this.ClientInnerIdentityHint);

        /// <summary>
        /// Reads and adds the user certificate to be installed along with the wlan profile
        /// </summary>
        /// <param name="filePath">path to the certificate file in question. PKCS12</param>
        /// <param name="passphrase">the passphrase to the certificate file in question</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        public AuthenticationMethod? WithClientCertificate(string filePath, string? passphrase = null)
            => this.EapType == EapType.TLS && VerifyCertificateBundle(filePath, passphrase)
                ? new AuthenticationMethod(
                    this.EapType,
                    this.InnerAuthType,
                    this.ServerCertificateAuthorities,
                    this.ServerNames,
                    this.ClientUserName,
                    this.ClientPassword,
                    Convert.ToBase64String(File.ReadAllBytes(filePath)),
                    passphrase,
                    this.ClientOuterIdentity,
                    this.ClientInnerIdentitySuffix,
                    this.ClientInnerIdentityHint)
                : null
                ;

        /// <summary>
        /// Sets the passphrase to use when derypting the certificate bundle.
        /// Will only be stored if valid.
        /// </summary>
        /// <param name="passphrase">the passphrase to the certificate</param>
        /// <returns>Clone of this object with the appropriate properties set</returns>
        public AuthenticationMethod? WithClientCertificatePassphrase(string passphrase)
            => this.EapType == EapType.TLS && VerifyCertificateBundle(this.ClientCertificateRaw, passphrase)
                ? new AuthenticationMethod(
                    this.EapType,
                    this.InnerAuthType,
                    this.ServerCertificateAuthorities,
                    this.ServerNames,
                    this.ClientUserName,
                    this.ClientPassword,
                    this.ClientCertificate,
                    passphrase,
                    this.ClientOuterIdentity,
                    this.ClientInnerIdentitySuffix,
                    this.ClientInnerIdentityHint)
                : null
                ;
        public AuthenticationMethod WithEapConfig(EapConfig eapConfig)
            => new(
                    eapConfig,
                    this.EapType,
                    this.InnerAuthType,
                    this.ServerCertificateAuthorities,
                    this.ServerNames,
                    this.ClientUserName,
                    this.ClientPassword,
                    this.ClientCertificate,
                    this.ClientCertificatePassphrase,
                    this.ClientOuterIdentity,
                    this.ClientInnerIdentitySuffix,
                    this.ClientInnerIdentityHint
                );

        /// <summary>
        /// Helper function which verifies if the
        /// certificate data and the passphrase is as valid combo
        /// </summary>
        /// <param name="rawCertificateData">Certificate data, PKCS12</param>
        /// <param name="passphrase">the passphrase to the certificate file in question</param>
        /// <returns>true if valid</returns>
        private static bool VerifyCertificateBundle(byte[] rawCertificateData, string? passphrase = null)
        {
            try
            {
                using var testCertificate = new X509Certificate2(rawCertificateData, passphrase);
            }
            catch (CryptographicException ex)
            {
                if ((ex.HResult & 0xFFFF) == 0x56)
                {
                    return false; // wrong passphrase
                }

                Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                Debug.Print(ex.ToString());
                Debug.Assert(false);
                throw;
            }
            return true;
        }

        /// <summary>
        /// Helper function which verifies if the filepath exists and
        /// that the passphrase is valid to read the certificate bundle
        /// </summary>
        /// <param name="filePath">path to the certificate file in question. PKCS12</param>
        /// <param name="passphrase">the passphrase to the certificate file in question</param>
        /// <returns>true if valid</returns>
        private static bool VerifyCertificateBundle(string filePath, string? passphrase = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(paramName: nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new ArgumentException(paramName: nameof(filePath), message: "file not found");
            }

            return VerifyCertificateBundle(File.ReadAllBytes(filePath), passphrase);
        }

        #endregion Credentials

        // Constructor
        public AuthenticationMethod(
            EapType eapType,
            InnerAuthType innerAuthType,
            List<string> serverCertificateAuthorities,
            List<string> serverName,
            string? clientUserName = null,
            string? clientPassword = null,
            string? clientCertificate = null,
            string? clientCertificatePassphrase = null,
            string? clientOuterIdentity = null,
            string? innerIdentitySuffix = null,
            bool innerIdentityHint = false
        ) : this(null, eapType, innerAuthType, serverCertificateAuthorities, serverName, clientUserName, clientPassword, clientCertificate, clientCertificatePassphrase, clientOuterIdentity, innerIdentitySuffix, innerIdentityHint) { }

        private AuthenticationMethod(
            EapConfig? eapConfig,
            EapType eapType,
            InnerAuthType innerAuthType,
            List<string> serverCertificateAuthorities,
            List<string> serverName,
            string? clientUserName = null,
            string? clientPassword = null,
            string? clientCertificate = null,
            string? clientCertificatePassphrase = null,
            string? clientOuterIdentity = null,
            string? innerIdentitySuffix = null,
            bool innerIdentityHint = false
        )
        {
            this.EapConfig = eapConfig;
            this.EapType = eapType;
            this.InnerAuthType = innerAuthType;
            this.ServerCertificateAuthorities = serverCertificateAuthorities ?? new List<string>();
            this.ServerNames = serverName ?? new List<string>();
            this.ClientUserName = clientUserName;
            this.ClientPassword = clientPassword;
            this.ClientCertificate = clientCertificate;
            this.ClientCertificatePassphrase = clientCertificatePassphrase;
            this.ClientOuterIdentity = clientOuterIdentity;
            this.ClientInnerIdentitySuffix = innerIdentitySuffix;
            this.ClientInnerIdentityHint = innerIdentityHint;
        }
    }
}
