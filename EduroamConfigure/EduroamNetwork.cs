using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using ManagedNativeWifi;
using ConfiguredWLANProfile = EduroamConfigure.PersistingStore.ConfiguredWLANProfile;

namespace EduroamConfigure
{
	/// <summary>
	/// Connects to an Eduroam network if available and stores information about it.
	/// Note: this struct is read only. After using it to store changes, fetch the networks again to see the changes.
	/// </summary>
	public readonly struct EduroamNetwork
	{
		public const string DefaultSsid = "eduroam";

		// Properties
		private AvailableNetworkPack NetworkPack { get; }
		private ProfilePack ProfilePack { get; }
		private ConfiguredWLANProfile? PersistedProfile { get; }

		public Guid InterfaceId { get; }
		public string ProfileName
		{ get => ProfilePack?.Name ?? NetworkPack?.ProfileName; }
		public bool IsAvailable
		{ get => NetworkPack != null; }

		// TODO: Add support for Wired 801x

		private EduroamNetwork(Guid interfaceId)
		{
			NetworkPack = null;
			PersistedProfile = null;
			ProfilePack = null;
			InterfaceId = interfaceId; // non-nullable
		}

		private EduroamNetwork(
			AvailableNetworkPack networkPack,
			ProfilePack profilePack,
			ConfiguredWLANProfile? persistedProfile = null)
			: this(profilePack?.Interface.Id ?? networkPack.Interface.Id)
		{
			Debug.Assert((networkPack, profilePack) != (null, null));
			Debug.Assert(
				networkPack == null
				|| profilePack == null
				|| networkPack.Interface.Id == profilePack.Interface.Id);

			NetworkPack = networkPack;
			ProfilePack = profilePack;
			PersistedProfile = persistedProfile;
		}

		/// <summary>
		/// Checks if eapConfig contains any supported authentification methods.
		/// If no such method exists, then warn the user before trying to install the config.
		/// </summary>
		/// <param name="eapConfig">The EAP config to check</param>
		/// <returns>True if it contains a supported type</returns>
		public static bool EapConfigIsSupported(EapConfig eapConfig)
		{
			_ = eapConfig ?? throw new ArgumentNullException(paramName: nameof(eapConfig));
			return eapConfig.AuthenticationMethods
				.Where(AuthMethodIsSupported).Any();
		}

		/// <summary>
		/// Checks if authMethod is supported.
		/// </summary>
		/// <param name="authMethod">The Authentification method to check</param>
		/// <returns>True if supported</returns>
		public static bool AuthMethodIsSupported(EapConfig.AuthenticationMethod authMethod)
		{
			_ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));
			return ProfileXml.IsSupported(authMethod)
				&& UserDataXml.IsSupported(authMethod);
		}

		/// <summary>
		/// Installs network profiles according to selected auth method.
		/// Will install multiple profile, one for each supported SSID
		/// Will overwrite any profiles with matching names if they exist.
		/// </summary>
		/// <param name="authMethod">TODO</param>
		/// <param name="forAllUsers">TODO</param>
		/// <returns>(success with ssid, success with hotspot2)</returns>
		public (bool, bool) InstallProfiles(EapConfig.AuthenticationMethod authMethod, bool forAllUsers = true)
		{
			_ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));

			PersistingStore.ProfileID = authMethod.EapConfig.Uid;

			var ssids = authMethod.EapConfig.CredentialApplicabilities
				.Where(cred => cred.NetworkType == IEEE802x.IEEE80211) // TODO: add support for Wired 802.1x
				.Where(cred => cred.MinRsnProto != "TKIP") // too insecure. // TODO: test user experience
				.Where(cred => cred.Ssid != null) // hs2 oid entires has no ssid
				.Select(cred => cred.Ssid)
				.ToList();

			bool installedSsid = false;
			foreach (var ssid in ssids)
			{
				(string profileName, string profileXml) = ProfileXml.CreateProfileXml(authMethod, withSsid: ssid);
				installedSsid |= InstallProfile(profileName, profileXml, isHs2: false, forAllUsers);
			}

			bool installedHs2 = false;
			if (authMethod.Hs2AuthMethod != null)
			{
				(string profileName, string profileXml) = ProfileXml.CreateProfileXml(authMethod.Hs2AuthMethod, asHs2Profile: true);
				installedHs2 = InstallProfile(profileName, profileXml, isHs2: true, forAllUsers);
			}

			return (installedSsid, installedHs2);
		}

		private bool InstallProfile(string profileName, string profileXml, bool isHs2, bool forAllUsers)
		{
			// security type not required
			const string securityType = null; // TODO: document why

			// overwrites if profile already exists
			const bool overwrite = true;

			// https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
			bool success = NativeWifi.SetProfile(
				InterfaceId,
				forAllUsers
					? ProfileType.AllUser
					: ProfileType.PerUser, // TODO: make this option work and set as default
				profileXml,
				securityType,
				overwrite);

			Debug.WriteLine("Install {2}WLANProfile {3} for '{0}' on {1}",
					profileName,
					InterfaceId,
					(isHs2) ? "Hs2 " : "",
					(success) ? "succeeded" : "failed");

			if (success) {
				PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
					.Add(new ConfiguredWLANProfile(InterfaceId, profileName, isHs2));
			}


			return success;
		}

		/// <summary>
		/// Sets user data (credentials) for a network profile.
		/// </summary>
		/// <param name="username">User's username optionally with realm</param>
		/// <param name="password">User's password.</param>
		/// <param name="authMethod">AuthMethod of installed profile</param>
		/// <param name="forAllUsers">TODO - mention the cert store thing</param>
		/// <returns>True if all succeeded, false if any failed or none was configured</returns>
		public bool InstallUserData(string username, string password, EapConfig.AuthenticationMethod authMethod, bool forAllUsers = false)
		{
			_ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));

			// See 'dwFlags' at: https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofileeapxmluserdata
			const uint profileUserTypeCurrentUsers = 0x00000000; // "current user" - https://github.com/rozmansi/WLANSetEAPUserData
			const uint profileUserTypeAllUSers = 0x00000001; // WLAN_SET_EAPHOST_DATA_ALL_USERS

			PersistingStore.Username = username; // save username

			bool ret = PersistingStore.ConfiguredWLANProfiles.Any();
			foreach (var configuredProfile in PersistingStore.ConfiguredWLANProfiles.ToList())
			{
				if (configuredProfile.InterfaceId != InterfaceId) continue;

				// generate user data xml file
				string userDataXml = UserDataXml.CreateUserDataXml(
					configuredProfile.IsHs2
						? authMethod.Hs2AuthMethod
						: authMethod,
					username,
					password);

				// install it
				var success = NativeWifi.SetProfileUserData(
					InterfaceId,
					configuredProfile.ProfileName,
					forAllUsers
						? profileUserTypeAllUSers
						: profileUserTypeCurrentUsers,
					userDataXml);
				ret &= success;

				// TODO: Of intrest: ProfilePack.Document.IsAutoConnectEnabled

				Debug.WriteLine("Installed {2}UserProfile {3} for '{0}' on {1}",
						configuredProfile.ProfileName,
						InterfaceId,
						configuredProfile.IsHs2 ? "Hs2 " : "",
						success ? "succeeded" : "failed");

				if (success && !configuredProfile.HasUserData) // ommit uneccesary writes
				{
					PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
						.Remove(configuredProfile)
						.Add(configuredProfile.WithUserDataSet());
				}
			}
			return ret;
		}

		/// <summary>
		/// Attempts to delete any previously installed network profiles
		/// </summary>
		/// <returns>True if ANY profile was deleted succesfully</returns>
		/// <remarks>
		/// True does not mean all the profiles has been deleted. Check IsConfigured ot verify this.
		/// </remarks>
		public bool RemoveInstalledProfiles()
		{
			var n = PersistingStore.ConfiguredWLANProfiles.Count;

			Debug.WriteLine("Removing installed profiles on " + InterfaceId);
			foreach (var configuredProfile in PersistingStore.ConfiguredWLANProfiles.ToList())
			{
				if (configuredProfile.InterfaceId == InterfaceId)
				{
					if (NativeWifi.DeleteProfile(InterfaceId, configuredProfile.ProfileName))
					{
						PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
							.Remove(configuredProfile);
					}
				}
			}

			return n != PersistingStore.ConfiguredWLANProfiles.Count || n == 0;
		}

		public async Task<bool> TryToConnect()
		{
			if (!IsAvailable) return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: NetworkPack.Interface.Id,
				profileName: NetworkPack.ProfileName,
				bssType: NetworkPack.BssType,
				timeout: TimeSpan.FromSeconds(8));
		}


		// static interface:

		/// <summary>
		/// Enumerates EduroamNetwork objects for all wireless network interfaces.
		/// </summary>
		/// <param name="eapConfig">Optional eap config to search with.</param>
		public static IEnumerable<EduroamNetwork> GetAll(EapConfig eapConfig)
		{
			// NativeWifi will throw if service is not available
			if (!IsWlanServiceApiAvailable())
				return Enumerable.Empty<EduroamNetwork>();
			PruneMissingPersistedProfiles();

			// TODO: multiple profiles on a single interface creates duplicate work further down
			//       perhaps group by InterfaceId and have a list of ProfileName in each EduroamNetwork?
			var availableNetworks = GetAllAvailableEduroamNetworkPacks(eapConfig?.CredentialApplicabilities)
				.Select(networkPack => new EduroamNetwork(networkPack, null))
				.ToList();

			var configuredInterfaces = availableNetworks
				.Select(network => network.NetworkPack.Interface.Id)
				.ToImmutableHashSet();

			// These are not available, but they are configurable
			var unavailableNetworks = GetAllInterfaceIds()
				.Where(guid => !configuredInterfaces.Contains(guid))
				.Select(guid => new EduroamNetwork(guid));

			return availableNetworks.Concat(unavailableNetworks);
		}

		/// <summary>
		/// Yields a EduroamNetwork instance for each configured profile for each network interface.
		/// </summary>
		public static IEnumerable<EduroamNetwork> GetConfigured()
		{
			// NativeWifi will throw if service is not available
			if (!IsWlanServiceApiAvailable())
				return Enumerable.Empty<EduroamNetwork>();
			PruneMissingPersistedProfiles();

			// join configured profiles
			return GetAllInstalledProfilePacksWithNetworkPacks()
				.Join(PersistingStore.ConfiguredWLANProfiles,
					networkPPack => (networkPPack.ppack.Name, networkPPack.ppack.Interface.Id),
					persitedProfile => (persitedProfile.ProfileName, persitedProfile.InterfaceId),
					(networkPPack, persitedProfile) =>
						new EduroamNetwork(networkPPack.network, networkPPack.ppack, persitedProfile))
				.OrderByDescending(network => network.IsAvailable);
		}

		/// <summary>
		/// "Can i install and connect to eduroam?"
		/// </summary>
		/// <param name="eapConfig">EAP config</param>
		/// <returns>true if eduroam is available</returns>
		public static bool IsEduroamAvailable(EapConfig eapConfig)
		{
			// NICE TO HAVE: some way to detect if any Hs2 hotspot is available if no matching ssid are found
			return GetAllAvailableEduroamNetworkPacks(eapConfig?.CredentialApplicabilities).Any();
		}

		private static void PruneMissingPersistedProfiles()
		{
			// look through installed profiles and remove persisted profile configurations which have been uninstalled by user
			var installedProfiles = GetAllInstalledProfilePacks().ToList();
			foreach (var configuredProfile in PersistingStore.ConfiguredWLANProfiles)
			{
				// if still installed
				if (installedProfiles.Any(ppack
						=> configuredProfile.InterfaceId == ppack.Interface.Id
						&& configuredProfile.ProfileName == ppack.Name))
					continue; // ignore

				// else remove
				Debug.WriteLine("Removing profile from persisting store called {0} on interface {1}",
					configuredProfile.ProfileName, configuredProfile.InterfaceId);
				PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
					.Remove(configuredProfile);
			}
		}

		/// <summary>
		/// Tries to access the wireless interfaces and reports wether the service is available or not
		/// If this returns false, then no interfaces nor packs will be available to configure
		/// </summary>
		/// <returns>True if wireless service is available</returns>
		private static bool IsWlanServiceApiAvailable()
		{
			try
			{
				NativeWifi.EnumerateInterfaces().ToList();
			}
			catch (TargetInvocationException ex) // we don't know why it gets wrapped
			{
				if (ex.GetBaseException().GetType().Name == "Win32Exception")
					if (ex.GetBaseException().Message == "MethodName: WlanOpenHandle, ErrorCode: 1062, ErrorMessage: The service has not been started.\r\n")
						return false;
				throw; // unknown
			}
			catch (Win32Exception ex) // in case it doesn't get wrapped in RELEASE
			{
				if (ex.NativeErrorCode == 1062) // ERROR_SERVICE_NOT_ACTIVE
					return false;
				throw; // unknown
			}
			return true;
		}


		/// <summary>
		/// Gets all available network packs with a profile configured
		/// </summary>
		/// <returns>Network packs</returns>
		private static List<AvailableNetworkPack> GetAllAvailableNetworkPacksWithProfiles()
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return new List<AvailableNetworkPack>();

			// TODO, maybe join in the profile pack?

			return NativeWifi.EnumerateAvailableNetworks()
				.Where(network => !string.IsNullOrEmpty(network.ProfileName))
				.ToList();
		}


		/// <summary>
		/// Gets all installed profile packs on the machine
		/// </summary>
		/// <returns>Profile packs</returns>
		private static IEnumerable<ProfilePack> GetAllInstalledProfilePacks()
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return Enumerable.Empty<ProfilePack>();

			// List all WLAN profiles installed on machine
			return NativeWifi.EnumerateProfiles();
		}

		/// <summary>
		/// Gets all installed profile packs on the machine along with their network packs if available
		/// </summary>
		/// <returns>Profile packs with optional network pack</returns>
		private static List<(ProfilePack ppack, AvailableNetworkPack network)> GetAllInstalledProfilePacksWithNetworkPacks()
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return new List<(ProfilePack, AvailableNetworkPack)>();

			// List all WLAN profiles installed on machine
			var allProfilePacks = NativeWifi.EnumerateProfiles().ToList();

			// inner join with available networks (in range)
			var availableProfileNetworks = allProfilePacks
				.Join(GetAllAvailableNetworkPacksWithProfiles(),
					ppack => (ppack.Name, ppack.Interface.Id),
					network => (network.ProfileName, network.Interface.Id),
					(ppack, network) => (ppack, network))
				.ToList();

			// create intermediate hash set of available profile packs, for quick lookups
			var availableProfilePacks = availableProfileNetworks
				.Select(item => item.ppack)
				.ToImmutableHashSet();

			// filter out the available profile packs from all profile packs
			var unavailableProfiles = allProfilePacks
				.Where(ppack => !availableProfilePacks.Contains(ppack))
				.Select(ppack => (ppack, (AvailableNetworkPack)null));

			return availableProfileNetworks.Concat(unavailableProfiles).ToList();
		}

		/// <summary>
		/// Gets all network packs containing information about an eduroam network, if any.
		/// </summary>
		/// <returns>Network packs</returns>
		private static List<AvailableNetworkPack> GetAllAvailableEduroamNetworkPacks(
			List<EapConfig.CredentialApplicability> credentialApplicabilities)
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return new List<AvailableNetworkPack>();

			if (credentialApplicabilities != null)
			{
				var ssids = credentialApplicabilities
					.Where(c => c.Ssid != null)
					.Select(c => c.Ssid)
					.ToImmutableHashSet();

				return NativeWifi.EnumerateAvailableNetworks()
					.Where(network => ssids.Contains(network.Ssid.ToString()))
					.OrderBy(network => string.IsNullOrEmpty(network.ProfileName))
					.ToList();
			}
			else
			{
				return NativeWifi.EnumerateAvailableNetworks()
					.Where(network => network.Ssid.ToString() == DefaultSsid)
					.OrderBy(network => string.IsNullOrEmpty(network.ProfileName))
					.ToList();
			}
		}

		/// <summary>
		/// Gets the computer's wireless network interface Ids, if they exists.
		/// </summary>
		/// <returns>all Wireless interface IDs</returns>
		private static IEnumerable<Guid> GetAllInterfaceIds()
		{
			return NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) // TODO: Wired 802.1x support
				.Where(nic => nic.Speed != -1) // lol
				.Select(nic => new Guid(nic.Id));
		}

	}
}
