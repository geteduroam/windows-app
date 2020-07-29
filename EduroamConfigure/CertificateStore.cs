using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using InstalledCertificate = EduroamConfigure.PersistingStore.InstalledCertificate;

namespace EduroamConfigure
{
	class CertificateStore
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
			if (IsCertificateInstalled(cert, storeName, storeLocation))
				return true;

			using var certStore = new X509Store(storeName, storeLocation);
			certStore.Open(OpenFlags.ReadWrite);

			Debug.WriteLine(string.Format("Writing '{0}' to cert store {1}:{2}",
				cert.FriendlyName, storeName.ToString(), storeLocation.ToString()));

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

			Debug.WriteLine(string.Format("Removing '{0}' from cert store {1}:{2}",
				cert.FriendlyName, storeName.ToString(), storeLocation.ToString()));

			using (var certStore = new X509Store(storeName, storeLocation))
			{
				certStore.Open(OpenFlags.ReadWrite);
				certStore.Remove(cert); // may produce a user prompt if the certstore in question is the root store
				certStore.Close();
			}

			// if we're still able to find it, then it probably wasn't removed.
			return !IsCertificateInstalled(cert, storeName, storeLocation);
		}
	}
}
