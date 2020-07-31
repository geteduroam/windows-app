using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using InstalledCertificate = EduroamConfigure.PersistingStore.InstalledCertificate;

namespace EduroamConfigure
{
	public static class CertificateStore
	{
        /// <summary>
        /// Installs the certificate into the certificate store chosen.
        /// If the certificate is sucessfully installed, this will be recorded in the Persistant Storage
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>False if the user declined</returns>
        public static bool InstallCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation) // TODO: move
        {
            _ = cert ?? throw new ArgumentNullException(paramName: nameof(cert));

            if (IsCertificateInstalled(cert, storeName, storeLocation))
                return true;

            using var certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadWrite);

            Debug.WriteLine("Writing '{0}' to cert store {1}:{2}",
                cert.FriendlyName, storeName.ToString(), storeLocation.ToString());

            try
            {
                // add to certificate store
                certStore.Add(cert);
                // ^ Will produce a popup prompt when installing to the root store 
                // if the certificate is not already installed
                // There fore you should predict this
                // and warn+instruct the user
            }
            catch (CryptographicException ex)
            {
                // if user selects No when prompted to install the CA
                if ((uint)ex.HResult == 0x800704C7) return false;

                throw; // unknown exception
            }

            // keep track of that we've installed it
            PersistingStore.InstalledCertificates = PersistingStore.InstalledCertificates
                .Add(InstalledCertificate.FromCertificate(cert, storeName, storeLocation));
            
            return true;
        }

        /// <summary>
        /// Checks if the certificate is installed into the chosen store
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>True if found</returns>
        public static bool IsCertificateInstalled(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation) // TODO: move
        {
            _ = cert ?? throw new ArgumentNullException(paramName: nameof(cert));

            using var certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            // check if already installed
            var matchingCerts = certStore.Certificates
                .Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);

            return matchingCerts.Count >= 1;
        }

        /// <summary>
        /// Checks if the certificate is installed into the chosen store
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>True if found</returns>
        public static bool UninstallCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation) // TODO: move
        {
            if (!IsCertificateInstalled(cert, storeName, storeLocation))
                return false;

            Debug.WriteLine("Removing '{0}' from cert store {1}:{2}",
                cert.FriendlyName, storeName.ToString(), storeLocation.ToString());

            using (var certStore = new X509Store(storeName, storeLocation))
            {
                certStore.Open(OpenFlags.ReadWrite);
                certStore.Remove(cert); // may produce a user prompt if the certstore in question is the root store
                certStore.Close();
            }

            // if we're still able to find it, then it probably wasn't removed.
            return !IsCertificateInstalled(cert, storeName, storeLocation);
        }

        public static IEnumerable<(X509Certificate2, InstalledCertificate)> EnumerateInstalledCertificates()
        {
            foreach (var installedCert in PersistingStore.InstalledCertificates.ToList())
            {
                // find matching certs in certstore
                X509Certificate2Collection matchingCerts;
                using (var certStore = new X509Store(installedCert.StoreName, installedCert.StoreLocation))
                {
                    certStore.Open(OpenFlags.ReadOnly);
                    matchingCerts = certStore.Certificates
                        .Find(X509FindType.FindByThumbprint, installedCert.Thumbprint, false);
                }

                bool found = false;
                foreach (var cert in matchingCerts)
                {
                    // thumbprint already found to match
                    // TODO: is it possible for these attributes to be modified after adding them to their stores?
                    if (cert.Issuer != installedCert.Issuer) continue;
                    if (cert.Subject != installedCert.Subject) continue;
                    if (cert.SerialNumber != installedCert.SerialNumber) continue;

                    found = true;
                    yield return (cert, installedCert);
                    break;
                }
                if (!found)
                {
                    // warning
                    if (matchingCerts.Count != 0)
                        Debug.Fail("Unable to find persisted certificate, even when thumbprint matched");

                    // not found, stop tracking it
                    PersistingStore.InstalledCertificates = PersistingStore.InstalledCertificates
                        .Remove(installedCert);
                }
            }
        }

        /// <summary>
        /// Uses the persistant storage to uninstall all known installed certificates
        /// </summary>
        /// <returns>true on success</returns>
        public static bool UninstallAllInstalledCertificates()
        {
            Debug.WriteLine("Uninstalling all installed certificates...");

            bool all_removed = true;
            foreach ((var cert, var installedCert) in EnumerateInstalledCertificates())
            {
                var success = UninstallCertificate(cert, installedCert.StoreName, installedCert.StoreLocation);

                if (success)
                    PersistingStore.InstalledCertificates = PersistingStore.InstalledCertificates
                        .Remove(installedCert);

                all_removed &= success;
            }

            // not transactionally secure, probably also not needed
            all_removed &= PersistingStore.InstalledCertificates.Count == 0;

            Debug.WriteLine("Uninstalling all installed certificates: " + (all_removed ? "SUCCESS": "FAILED"));
            Debug.WriteLine("");
            return all_removed;
        }
    }
}
