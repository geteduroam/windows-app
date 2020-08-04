using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace EduroamConfigure
{
    /// <summary>
    /// This is a static class with properties which access the persistent storage, which in this implementation is the windows registry.
    /// All the entries are immutable, to force you to store all your changes properly.
    /// </summary>
    public static class PersistingStore
    {
        /// <summary>
        /// The username to remember from when the user last logged in
        /// </summary>
        public static string Username
        {
            get => GetValue<string>("Username");
            set => SetValue<string>("Username", value);
        }

        /// <summary>
        /// The ID of the eap-config profile as assigned by discovery.geteduroam.*
        /// </summary>
        public static string ProfileID
        {
            get => GetValue<string>("ProfileID");
            set => SetValue<string>("ProfileID", value);
        }

        /// <summary>
        /// A set of the configured WLANProfiles
        /// </summary>
        public static ImmutableHashSet<ConfiguredWLANProfile> ConfiguredWLANProfiles
        {
            get => GetValue<ImmutableHashSet<ConfiguredWLANProfile>>("ConfiguredWLANProfiles", "[]");
            set => SetValue<ImmutableHashSet<ConfiguredWLANProfile>>("ConfiguredWLANProfiles", value);
        }

        /// <summary>
        /// A set of the installed CAs and client certificates.
        /// Managed by EduroamConfigure.CertificateStore
        /// </summary>
        public static ImmutableHashSet<InstalledCertificate> InstalledCertificates
        {
            get => GetValue<ImmutableHashSet<InstalledCertificate>>("InstalledCertificates", "[]");
            set => SetValue<ImmutableHashSet<InstalledCertificate>>("InstalledCertificates", value);
        }

        /// <summary>
        /// The endpoints to access the lets-wifi
        /// using the refresh token
        /// </summary>
        public static (string profileId, Uri tokenEndpoint, Uri eapEndpoint)? LetsWifiEndpoints
        {
            get => GetValue<(string, Uri, Uri)?>("LetsWifiEndpoints");
            set => SetValue<(string, Uri, Uri)?>("LetsWifiEndpoints", value);
        }

        /// <summary>
        /// The single-use refresh token to talk with the lets-wifi API
        /// </summary>
        public static string LetsWifiRefreshToken
        {
            // TODO: perhaps encrypt this in some fashion?
            // https://stackoverflow.com/questions/32548714/how-to-store-and-retrieve-credentials-on-windows-using-c-sharp
            get => GetValue<string>("LetsWifiRefreshToken");
            set => SetValue<string>("LetsWifiRefreshToken", value);
        }

        public readonly struct ConfiguredWLANProfile
        {
            public Guid   InterfaceId { get; }
            public string ProfileName { get; }
            public bool   IsHs2       { get; }
            public bool   HasUserData { get; }

            public ConfiguredWLANProfile(Guid interfaceId, string profileName, bool isHs2, bool hasUserData = false)
            {
                InterfaceId = interfaceId;
                ProfileName = profileName;
                IsHs2       = isHs2;
                HasUserData = hasUserData;
            }

            public ConfiguredWLANProfile WithUserDataSet()
                => new ConfiguredWLANProfile(
                    interfaceId: InterfaceId,
                    profileName: ProfileName,
                    isHs2:       IsHs2,
                    hasUserData: true);
        }

        public readonly struct InstalledCertificate
        {
            public StoreName     StoreName     { get; }
            public StoreLocation StoreLocation { get; }
            public string        Thumbprint    { get; }
            public string        SerialNumber  { get; }
            public string        Subject       { get; }
            public string        Issuer        { get; }
            public DateTime      NotBefore     { get; }
            public DateTime      NotAfter      { get; }

            public InstalledCertificate(
                StoreName     storeName,
                StoreLocation storeLocation,
                string        thumbprint,
                string        serialNumber,
                string        subject,
                string        issuer,
                DateTime      notBefore,
                DateTime      notAfter)
            {
                StoreName     = storeName;
                StoreLocation = storeLocation;
                Thumbprint    = thumbprint;
                SerialNumber  = serialNumber;
                Subject       = subject;
                Issuer        = issuer;
                NotBefore     = notBefore;
                NotAfter      = notAfter;
            }

            public static InstalledCertificate FromCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
                 => cert == null
                    ? throw new ArgumentNullException(paramName: nameof(cert))
                    : new InstalledCertificate(
                        storeName:     storeName,
                        storeLocation: storeLocation,
                        thumbprint:    cert.Thumbprint,
                        serialNumber:  cert.SerialNumber,
                        subject:       cert.Subject,
                        issuer:        cert.Issuer,
                        notBefore:     cert.NotBefore,
                        notAfter:      cert.NotAfter);
        }


        // Inner workings:

        private const string ns = "HKEY_CURRENT_USER\\Software\\GetEduroam"; // Namespace in Registry
        private static T GetValue<T>(string key, string defaultJson = "null")
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(
                    (string)Registry.GetValue(ns, key, null) ?? defaultJson);
            }
            catch (JsonReaderException)
            {
                return JsonConvert.DeserializeObject<T>(defaultJson);
            }
        }
        private static void SetValue<T>(string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            if (serialized != (string)Registry.GetValue(ns, key, null)) // only write when we make a change
            {
                Debug.WriteLine("Write to {0}\\{1}: {2}", ns, key, serialized);
                Registry.SetValue(ns, key, serialized);
            }

            return;
        }
    }
}
