using EduRoam.Connect.Exceptions;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


using InstalledCertificate = EduRoam.Connect.PersistingStore.InstalledCertificate;

namespace EduRoam.Connect.Install
{
    public static class CertificateStore
    {
        // Certificate stores:

        // Used to install root CAs to verify server certificates with
        public readonly static StoreName RootCaStoreName = StoreName.Root;
        public readonly static StoreLocation RootCaStoreLocation = StoreLocation.CurrentUser; // NICE TO HAVE: make this configurable to LocalMachine
                                                                                              // Used to install CAs to verify server certificates with
        public readonly static StoreName InterCaStoreName = StoreName.CertificateAuthority;
        public readonly static StoreLocation InterCaStoreLocation = StoreLocation.CurrentUser; // NICE TO HAVE: make this configurable to LocalMachine
                                                                                               // Used to install TLS client certificates
        public readonly static StoreName UserCertStoreName = StoreName.My;
        public readonly static StoreLocation UserCertStoreLocation = StoreLocation.CurrentUser;

        /// <summary>
        /// Installs the certificate into the certificate store chosen.
        /// If the certificate is sucessfully installed, this will be recorded in the Persistant Storage
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>False if the user declined</returns>
        public static void InstallCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            _ = cert ?? throw new ArgumentNullException(paramName: nameof(cert));

            if (IsCertificateInstalled(cert, storeName, storeLocation))
                return;

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
                if ((uint)ex.HResult == 0x800704C7)
                    throw new UserAbortException("User selected No when prompted for certificate");

                Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                Debug.Print(ex.ToString());
                Debug.Assert(false);
                throw; // unknown exception
            }

            // keep track of that we've installed it
            PersistingStore.InstalledCertificates = PersistingStore.InstalledCertificates
                .Add(InstalledCertificate.FromCertificate(cert, storeName, storeLocation));
        }

        /// <summary>
        /// Checks if the certificate is installed into the chosen store
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>True if found</returns>
        public static bool IsCertificateInstalled(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            _ = cert ?? throw new ArgumentNullException(paramName: nameof(cert));

            using var certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            // check if already installed
            var matchingCerts = certStore.Certificates
                .Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);

            return matchingCerts.Count >= 1;
        }

        public static bool IsCertificateInstalledByUs(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
            => IsCertificateInstalled(cert, storeName, storeLocation)
            && PersistingStore.InstalledCertificates
                .Contains(InstalledCertificate.FromCertificate(cert, storeName, storeLocation));

        public static bool AnyRootCaInstalledByUs()
            => EnumerateInstalledCertificates()
                .Where(cert => IsCertificateInstalledByUs(cert.cert, cert.installedCert.StoreName, cert.installedCert.StoreLocation))
                .Any(cert => cert.installedCert.StoreName == StoreName.Root);

        /// <summary>
        /// Checks if the certificate is installed into the chosen store
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="storeName">The certificate store to use</param>
        /// <param name="storeLocation">The location within the certificate store to use</param>
        /// <returns>True if found</returns>
        public static bool UninstallCertificate(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            if (!IsCertificateInstalled(cert, storeName, storeLocation))
                return false;

            Debug.WriteLine("Removing '{0}' from cert store {1}:{2}",
                cert.FriendlyName, storeName.ToString(), storeLocation.ToString());

            using var certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadWrite);

            try
            {
                // remove from certificate store
                certStore.Remove(cert);
                // ^ Will produce a popup prompt when installing to the root store
                // if the certificate is not already installed
                // There fore you should predict this
                // and warn+instruct the user
            }
            catch (CryptographicException ex)
            {
                // if user selects No when prompted to remove the CA
                if ((uint)ex.HResult == 0x800704C7) return false;

                Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                Debug.Print(ex.ToString());
                Debug.Assert(false);
                throw; // unknown exception
            }


            // if we're still able to find it, then it probably wasn't removed.
            return !IsCertificateInstalled(cert, storeName, storeLocation);
            // TODO: ^ might cause false negatives in the case where the cert came from LOCAL MACHINE, more testing needed
        }

        public static IEnumerable<(X509Certificate2 cert, InstalledCertificate installedCert)> EnumerateInstalledCertificates()
        {
            foreach (var installedCert in PersistingStore.InstalledCertificates.ToList())
            {
                // find matching certs in certstore
                X509Certificate2Collection matchingCerts;
                using (var certStore = new X509Store(installedCert.StoreName, installedCert.StoreLocation))
                {
                    certStore.Open(OpenFlags.ReadOnly);
                    matchingCerts = certStore.Certificates
                        .Find(X509FindType.FindByThumbprint, installedCert.Thumbprint, validOnly: false);
                }

                bool found = false;
                foreach (var cert in matchingCerts)
                {
                    // thumbprint already found to match
                    // TODO: is it possible for these attributes to be modified after adding them to their stores? If not then remove the RELEASE block below
                    if (cert.Issuer != installedCert.Issuer) continue;
                    if (cert.Subject != installedCert.Subject) continue;
                    if (cert.SerialNumber != installedCert.SerialNumber) continue;

                    found = true;
                    yield return (cert, installedCert);
                    break;
                }
#if !DEBUG
				if (!found && matchingCerts.Count == 1) // fields didn't match, but the fingerprint does
				{
					found = true;
					yield return (matchingCerts[0], installedCert);
				}
#endif
                if (!found)
                {
                    // warning
                    // TODO: prime target for metrics
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
        public static bool UninstallAllInstalledCertificates(bool omitRootCa, bool abortOnFail = false, bool omitNotInstalledByUs = true)
        {
            Debug.WriteLine("Uninstalling all installed certificates...");

            bool all_removed = true;
            foreach ((var cert, var installedCert) in EnumerateInstalledCertificates())
            {
                if (installedCert.StoreName == StoreName.Root && omitRootCa)
                    continue; // skip
                if (!IsCertificateInstalledByUs(cert, installedCert.StoreName, installedCert.StoreLocation) && omitNotInstalledByUs)
                    continue; // skip

                var success = UninstallCertificate(cert, installedCert.StoreName, installedCert.StoreLocation);

                if (success)
                    PersistingStore.InstalledCertificates = PersistingStore.InstalledCertificates
                        .Remove(installedCert);

                all_removed &= success;

                if (!success && abortOnFail)
                    break;
            }

            // not transactionally secure, probably also not needed
            all_removed &= PersistingStore.InstalledCertificates.Count == 0;

            Debug.WriteLine("Uninstalling all installed certificates: " + (all_removed ? "SUCCESS" : "FAILED"));
            Debug.WriteLine("");
            return all_removed;
        }

        public static bool CertificateIsRootCA(X509Certificate2 cert)
            => cert != null && cert?.Subject == cert?.Issuer; // If this doesn't work, try https://stackoverflow.com/a/34174890
    }
}
