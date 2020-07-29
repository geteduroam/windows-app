using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace EduroamConfigure
{
	/// <summary>
	/// This is a class with static properties which access the persistent storage in the windows registry.
	/// All the entries are immutable, to force you to store all your changes properly.
	/// </summary>
	public class PersistingStore
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
		public static ImmutableHashSet<ConfiguredProfile> ConfiguredProfiles
		{
			get => GetValue<ImmutableHashSet<ConfiguredProfile>>("ConfigureProfiles", "[]");
			set => SetValue<ImmutableHashSet<ConfiguredProfile>>("ConfigureProfiles", value);
		}

		/// <summary>
		/// A set of the installed CAs and client certificates
		/// </summary>
		public static ImmutableHashSet<InstalledCertificate> InstalledCertificates
		{
			get => GetValue<ImmutableHashSet<InstalledCertificate>>("InstalledCertificates", "[]");
			set => SetValue<ImmutableHashSet<InstalledCertificate>>("InstalledCertificates", value);
		}

		public readonly struct ConfiguredProfile
		{
			public Guid   InterfaceId { get; }
			public string ProfileName { get; }
			public bool   IsHs2       { get; }
			public bool   HasUserData { get; }

			public ConfiguredProfile(Guid interfaceId, string profileName, bool isHs2, bool hasUserData = false)
			{
				InterfaceId = interfaceId;
				ProfileName = profileName;
				IsHs2       = isHs2;
				HasUserData = hasUserData;
			}

			public ConfiguredProfile WithUserDataSet()
				=> new ConfiguredProfile(
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
				 => new InstalledCertificate(
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

		private const string ns = "HKEY_CURRENT_USER\\GetEduroam"; // Namespace in Registry
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
				Debug.WriteLine(string.Format("Write to {0}\\{1}: {2}", ns, key, serialized));
				Registry.SetValue(ns, key, serialized);
			}

			return;
		}
	}
}
