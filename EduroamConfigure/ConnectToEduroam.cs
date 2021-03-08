using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Diagnostics;

namespace EduroamConfigure
{
	/// <summary>
	/// Contains various functions for:
	/// - installing certificates
	/// - creating a wireless profile
	/// - setting user data
	/// - connecting to a network
	/// </summary>
	public static class ConnectToEduroam
	{
		// Certificate stores:

		// Used to install root CAs to verify server certificates with
		private const StoreName rootCaStoreName = StoreName.Root;
		private const StoreLocation rootCaStoreLocation = StoreLocation.CurrentUser; // NICE TO HAVE: make this configurable to LocalMachine
		// Used to install CAs to verify server certificates with
		private const StoreName interCaStoreName = StoreName.CertificateAuthority;
		private const StoreLocation interCaStoreLocation = StoreLocation.CurrentUser; // NICE TO HAVE: make this configurable to LocalMachine
		// Used to install TLS client certificates
		private const StoreName userCertStoreName = StoreName.My;
		private const StoreLocation userCertStoreLocation = StoreLocation.CurrentUser;

		/// <summary>
		/// Checks the EAP config to see if there is any issues
		/// TODO: test this
		/// TODO: use this in ui
		/// </summary>
		/// <returns>A tuple on the form: (bool isCritical, string description)</returns>
		public static IEnumerable<ValueTuple<bool, string>> LookForWarningsInEapConfig(EapConfig eapConfig)
		{
			_ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));

			if (!EduroamNetwork.IsEapConfigSupported(eapConfig))
			{
				yield return (true, "This configuration is not supported");
				yield break;
			}

			if (!eapConfig.AuthenticationMethods
					.Where(EduroamNetwork.IsAuthMethodSupported)
					.All(authMethod => authMethod.ServerCertificateAuthorities.Any()))
				yield return (true, "This configuration is missing Certificate Authorities");

			var CAs = EnumerateCAs(eapConfig).ToList();

			DateTime now = DateTime.Now;
			bool has_expired_ca = CAs
				.Any(caCert => caCert.NotAfter < now);

			bool has_a_yet_to_expire_ca = CAs
				.Any(caCert => now < caCert.NotAfter);

			bool has_valid_ca = CAs
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
				DateTime earliest = CAs
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
			return eapConfig.AuthenticationMethods
				.Where(EduroamNetwork.IsAuthMethodSupported)
				.SelectMany(authMethod => authMethod.CertificateAuthoritiesAsX509Certificate2())
				.Where(CertificateStore.CertificateIsRootCA)
				.GroupBy(cert => cert.Thumbprint, (key, certs) => certs.FirstOrDefault()); // distinct, alternative is to use DistinctBy in MoreLINQ
		}

		/// <summary>
		/// Enumerates the CAs which the eapConfig in question defines, wrapped a install helper class
		/// </summary>
		public static IEnumerable<CertificateInstaller> EnumerateCAInstallers(EapConfig eapConfig)
		{
			_ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
			return EnumerateCAs(eapConfig)
				.Select(cert => new CertificateInstaller(cert, rootCaStoreName, rootCaStoreLocation));
		}

		/// <summary>
		/// A helper class which helps you ensure a single certificates is installed.
		/// </summary>
		public class CertificateInstaller
		{
			private readonly X509Certificate2 cert;
			private readonly StoreName storeName;
			private readonly StoreLocation storeLocation;

			public CertificateInstaller(
				X509Certificate2 cert,
				StoreName storeName,
				StoreLocation storeLocation)
			{
				this.cert = cert ?? throw new ArgumentNullException(paramName: nameof(cert));
				this.storeLocation = storeLocation;
				this.storeName = storeName;
			}

			override public string ToString()
				=> cert.FriendlyName;

			public bool IsCa { get => storeName == rootCaStoreName; }

			public bool IsInstalled
			{
				get => CertificateStore.IsCertificateInstalled(cert, storeName, storeLocation);
			}
			public bool IsInstalledByUs
			{
				get => CertificateStore.IsCertificateInstalledByUs(cert, storeName, storeLocation);
			}

			public void InstallCertificate()
				=> CertificateStore.InstallCertificate(cert, storeName, storeLocation);
		}

		/// <summary>
		/// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
		/// Refer to frmSummary.InstallEapConfig to see how to use it (TODO: actually explain when finalized)
		/// </summary>
		/// <param name="eapConfig">EapConfig object</param>
		/// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
		public static IEnumerable<EapAuthMethodInstaller> InstallEapConfig(EapConfig eapConfig)
			=> eapConfig == null
				? throw new ArgumentNullException(paramName: nameof(eapConfig))
				: eapConfig.AuthenticationMethods
					.Where(EduroamNetwork.IsAuthMethodSupported)
					.Select(authMethod => new EapAuthMethodInstaller(authMethod));

		/// <summary>
		/// A class which helps you install one of the authMethods
		/// in a EapConfig, designed to be interactive wiht the user.
		/// </summary>
		public class EapAuthMethodInstaller
		{
			// To track proper order of operations
			private bool HasInstalledCertificates = false;

			// reference to the EAP config
			public EapConfig.AuthenticationMethod AuthMethod { get; }


			/// <summary>
			/// Constructs a EapAuthMethodInstaller
			/// </summary>
			/// <param name="authMethod">The authentification method to attempt to install</param>
			public EapAuthMethodInstaller(EapConfig.AuthenticationMethod authMethod)
			{
				AuthMethod = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));
			}

			/// <summary>
			/// Will install root CAs, intermediate CAs and user certificates provided by the authMethod.
			/// Installing a root CA in windows will produce a dialog box which the user must accept.
			/// This will quit partway through if the user refuses to install any CA, but it is safe to run again.
			/// Use EnumerateCAInstallers to have the user install the CAs in a controlled manner before installing the EAP config
			/// </summary>
			/// <returns>Returns true if all certificates has been successfully installed</returns>
			public void InstallCertificates()
			{
				if (AuthMethod.NeedsClientCertificate())
					throw new EduroamAppUserException("no client certificate was provided");

				// get all CAs from Authentication method
				foreach (var cert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
				{
					// if this doesn't work, try https://stackoverflow.com/a/34174890
					bool isRootCA = cert.Subject == cert.Issuer;
					CertificateStore.InstallCertificate(cert,
						isRootCA ? rootCaStoreName : interCaStoreName,
						isRootCA ? rootCaStoreLocation : interCaStoreLocation);
				}

				// Install client certificate if any
				if (!string.IsNullOrEmpty(AuthMethod.ClientCertificate))
				{
					using var clientCert = AuthMethod.ClientCertificateAsX509Certificate2();
					CertificateStore.InstallCertificate(clientCert, userCertStoreName, userCertStoreLocation);
				}

				HasInstalledCertificates = true;
			}

			/// <summary>
			/// Will install the authMethod as a profile
			/// Having run InstallCertificates successfully before calling this is a prerequisite
			/// If this returns FALSE: It means there is a missing TLS client certificate left to be installed
			/// </summary>
			/// <returns>True if the profile was installed on any interface</returns>
			public void InstallWLANProfile(string username=null, string password=null)
			{
				if (!HasInstalledCertificates)
					throw new EduroamAppUserException("missing certificates",
						"You must first install certificates with InstallCertificates");

				// Install wlan profile
				foreach (var network in EduroamNetwork.GetAll(AuthMethod.EapConfig))
				{
					network.InstallProfiles(AuthMethod);
				}

				// Debug output
				Debug.WriteLine("Ssid profile eap type: " + AuthMethod.EapType.ToString() ?? "None");
				Debug.WriteLine("Hs2  profile eap type: " + AuthMethod.Hs2AuthMethod?.EapType.ToString() ?? "None");

				// sets user data
				Debug.WriteLine("Install user profiles for user {0}", username);
				foreach (var network in EduroamNetwork.GetAll(AuthMethod.EapConfig))
				{
					network.InstallUserData(username, password, AuthMethod);
				}
			}

			public (DateTime From, DateTime? To) GetTimeWhenValid()
			{
				using var cert = AuthMethod.ClientCertificateAsX509Certificate2();
				return cert == null
					? (DateTime.Now.AddSeconds(-30), (DateTime?)null)
					: (cert.NotBefore, cert.NotAfter);
			}

		}


		/// <summary>
		/// Deletes all network profile matching ssid, which is "eduroam" by default
		/// </summary>
		/// <returns>True if all profile deletions were succesful</returns>
		public static void RemoveAllWLANProfiles()
		{
			Exception ex = null;
			foreach (EduroamNetwork network in EduroamNetwork.GetAll(null))
			{
				try
				{
					network.RemoveInstalledProfiles();
				} catch (Exception e)
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
			foreach (var network in EduroamNetwork.GetConfigured())
			{
				var success = await network.TryToConnect();
				if (success) return true;
			}
			return false;
		}

	}

}
