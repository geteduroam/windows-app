using EduRoam.Connect.Eap;
using EduRoam.Connect.Install;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduRoam.Connect
{
    /// <summary>
    /// Contains various functions for:
    /// - installing certificates
    /// - creating a wireless profile
    /// - setting user data
    /// - connecting to a network
    /// </summary>
    internal static partial class ConnectToEduroam
    {
        /// <summary>
        /// Enumerates the CAs which the eapConfig in question defines, wrapped a install helper class
        /// </summary>
        internal static IEnumerable<CertificateInstaller> EnumerateCAInstallers(EapConfig eapConfig)
        {
            _ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));

            var rootCACertificates = eapConfig.AuthenticationMethods
                .Where(EduRoamNetwork.IsAuthMethodSupported)
                // If we use thumbprints, we won't install any root CA
                // Server verification is done by fingerprint instead,
                .Where(authMethod => authMethod.CertificateThumbprints.Count == 0)
                .SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
                .Where(CertificateStore.CertificateIsRootCA);

            return rootCACertificates
                .GroupBy(cert => cert.Thumbprint, (key, certs) => certs.FirstOrDefault()) // distinct, alternative is to use DistinctBy in MoreLINQ
                .Select(cert => new CertificateInstaller(cert, CertificateStore.RootCaStoreName, CertificateStore.CertStoreLocation));
        }

        /// <summary>
        /// Deletes all network profile matching ssid, which is "eduroam" by default
        /// </summary>
        /// <returns>True if all profile deletions were succesful</returns>
        internal static void RemoveAllWLANProfiles()
        {
            Exception? ex = null;
            var allNetworks = EduRoamNetwork.GetAll();
            foreach (var network in allNetworks)
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

            if (ex != null)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to connects to any eduroam wireless LAN, in succession
        /// </summary>
        /// <returns>True if successfully connected. False if not.</returns>
        internal static async Task<bool> TryToConnect()
        {
            // gets updated eduroam network packs
            foreach (var network in EduRoamNetwork.GetConfigured())
            {
                if (await network.TryToConnect())
                    return true;
            }
            return false;
        }

    }

}
