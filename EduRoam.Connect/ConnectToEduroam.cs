using EduRoam.Connect.Install;

using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect
{
    /// <summary>
    /// Contains various functions for:
    /// - installing certificates
    /// - creating a wireless profile
    /// - setting user data
    /// - connecting to a network
    /// </summary>
    public static partial class ConnectToEduroam
    {
        /// <summary>
        /// Checks the EAP config to see if there is any issues
        /// TODO: test this
        /// TODO: use this in ui
        /// </summary>
        /// <returns>A tuple on the form: (bool isCritical, string description)</returns>
        public static IEnumerable<ValueTuple<bool, string>> LookForWarningsInEapConfig(EapConfig eapConfig)
        {
            _ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));

            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                yield return (true, "This configuration is not supported");
                yield break;
            }

            if (!eapConfig.AuthenticationMethods
                    .Where(EduRoamNetwork.IsAuthMethodSupported)
                    .All(authMethod => authMethod.ServerCertificateAuthorities.Any()))
                yield return (true, "This configuration is missing Certificate Authorities");

            var CAs = EnumerateCAs(eapConfig).ToList();

            var now = DateTime.Now;
            var has_expired_ca = CAs
                .Any(caCert => caCert.NotAfter < now);

            var has_a_yet_to_expire_ca = CAs
                .Any(caCert => now < caCert.NotAfter);

            var has_valid_ca = CAs
                .Where(caCert => now < caCert.NotAfter)
                .Any(caCert => caCert.NotBefore < now);

            if (has_expired_ca)
            {
                yield return has_valid_ca
                    ? (false,
                        "One of the provided Certificate Authorities from this institution has expired.\r\n" +
                        "There might be some issues connecting to eduroam.")
                    : (true,
                        "The provided Certificate Authorities from this institution have all expired!\r\n" +
                        "Please contact the institution to have the issue fixed!");
            }
            else if (!has_valid_ca && has_a_yet_to_expire_ca)
            {
                var earliest = CAs
                    .Where(caCert => now < caCert.NotAfter)
                    .Max(caCert => caCert.NotBefore);

                yield return (false,
                    "The Certificate Authorities in this configuration has yet to become valid.\r\n" +
                    "This configuration will become valid in " + (earliest - now).TotalMinutes + " minutes.");
            }
            else if (!has_valid_ca)
            {
                yield return (false,
                    "The Certificate Authorities in this configuration are not valid.");
            }

            CAs.ForEach(cert => cert.Dispose());
        }

        /// <summary>
        /// Enumerates the CAs which the eapConfig in question defines
        /// </summary>
        private static IEnumerable<X509Certificate2> EnumerateCAs(EapConfig eapConfig)
        {
            _ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
            var rootCACertificates = eapConfig.AuthenticationMethods
                .Where(EduRoamNetwork.IsAuthMethodSupported)
                .SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
                .Where(CertificateStore.CertificateIsRootCA);

            return rootCACertificates
                .DistinctBy(cert => cert.Thumbprint);
            // .GroupBy(cert => cert!.Thumbprint, (key, certs) => certs.FirstOrDefault()); // distinct, alternative is to use DistinctBy in MoreLINQ
        }

        /// <summary>
        /// Enumerates the CAs which the eapConfig in question defines, wrapped a install helper class
        /// </summary>
        public static IEnumerable<CertificateInstaller> EnumerateCAInstallers(EapConfig eapConfig)
        {
            _ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
            return EnumerateCAs(eapConfig)
                .Select(cert => new CertificateInstaller(cert, CertificateStore.RootCaStoreName, CertificateStore.RootCaStoreLocation));
        }


        /// <summary>
        /// Deletes all network profile matching ssid, which is "eduroam" by default
        /// </summary>
        /// <returns>True if all profile deletions were succesful</returns>
        public static void RemoveAllWLANProfiles()
        {
            Exception? ex = null;
            foreach (var network in EduRoamNetwork.GetAll())
            {
                try
                {
                    network.RemoveInstalledProfiles();
                }
                catch (ArgumentException e)
                {
                    ex = e;
                }
            }

            if (ex != null) throw ex;
        }


        /// <summary>
        /// Attempts to connects to any eduroam wireless LAN, in succession
        /// </summary>
        /// <returns>True if successfully connected. False if not.</returns>
        public static async Task<bool> TryToConnect()
        {
            // gets updated eduroam network packs
            foreach (var network in EduRoamNetwork.GetConfigured())
            {
                var success = await network.TryToConnect();
                if (success) return true;
            }
            return false;
        }

    }

}
