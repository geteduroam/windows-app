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
		public static bool IsEapConfigSupported(EapConfig eapConfig)
		{
			return eapConfig?.AuthenticationMethods
				.Where(IsAuthMethodSupported).Any()
				?? false;
		}

		/// <summary>
		/// Checks if authMethod is supported.
		/// </summary>
		/// <param name="authMethod">The Authentification method to check</param>
		/// <returns>True if supported</returns>
		public static bool IsAuthMethodSupported(EapConfig.AuthenticationMethod authMethod)
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
		/// <param name="username">User's username optionally with realm</param>
		/// <param name="password">User's password.</param>
		/// <param name="authMethod">AuthMethod of profiles to be installed</param>
		/// <param name="forAllUsers">Install for all users or only the current user</param>
		/// <returns>(success with ssid, success with hotspot2)</returns>
		/// <remarks><paramref name="forAllUsers"/> is ignored because it currently must be true for profiles and false for eapxml</remarks>
		public void InstallProfiles(EapConfig.AuthenticationMethod authMethod, string username = null, string password = null, bool forAllUsers = true)
		{
			_ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));

			PersistingStore.IdentityProvider = PersistingStore.IdentityProviderInfo.From(authMethod);
			PersistingStore.Username = username; // save username

			var ssids = authMethod.SSIDs;

			ConfiguredWLANProfile profile;
			foreach (var ssid in ssids)
			{
				(string profileName, string profileXml) = ProfileXml.CreateSSIDProfileXml(authMethod, ssid);
				string userDataXml = UserDataXml.CreateUserDataXml(authMethod, username, password);
				try
				{
					// forAllUsers must be true when installing the profile, but false when installing userdata
					// Otherwise the profile is installed but doens't work.  We don't know why.
					profile = InstallProfile(profileName, profileXml, hs20: false, forAllUsers: true);
					// forAllUsers does not work with EAP-TLS, probably because the certificate is in the personal store
					// Same for all methods where the CA is not public
					InstallUserData(profile, userDataXml, false);
				}
				catch (Win32Exception e)
				{
					Debug.Print(e.ToString());
					throw;
				}
			}

			if (authMethod.IsHS20Supported)
			{
				(string profileName, string profileXml) = ProfileXml.CreateHS20ProfileXml(authMethod);
				string userDataXml = UserDataXml.CreateUserDataXml(authMethod, username, password);
				try
				{
					// forAllUsers must be true when installing the profile, but false when installing userdata
					// Otherwise the profile is installed but doens't work.  We don't know why.
					profile = InstallProfile(profileName, profileXml, hs20: true, forAllUsers: true);
					// forAllUsers does not work with EAP-TLS, probably because the certificate is in the personal store
					// Same for all methods where the CA is not public
					InstallUserData(profile, userDataXml, forAllUsers: false);
				}
				catch (Win32Exception e)
				{
					Debug.Print(e.ToString());

					// ABANDON HOPE, ALL YE WHO ENTER HERE.

					// We have observed that some devices, especially USB Wi-Fi dongles,
					// will simply fail when trying to configure HS20 on their interfaces.
					// The error is BAD PROFILE, but the profile works fine on most PCI-based adapters.
					// Since most profiles offer both HS20 and SSID, and we assume that these adapters
					// not accepting HS20 is due to them not supporting HS20 anyway,
					// the best user experience is to silently discard these errors.
					// However, we do so ONLY if SSIDs are also set, AND we are not running in DEBUG mode.

					// If any errors occur but no SSIDs were configured,
					// we throw an exception, even if it was ignored otherwise
					if (ssids.Count == 0)
					{
						throw;
					}

#if !DEBUG
					// Accept any error when ssids.Count > 0 for Release version.
					// We still are not sure what kind of errors are to be expected
					// when configuring HS20, so this gives the best user experience
					return;
#endif

					// -2147467259 == 0x80004005, which is the most generic error code Windows has to offer
					// Useless but fun reading:
					// https://support.microsoft.com/en-us/windows/fix-error-0x80004005-9acfca89-b5e4-b976-6fa1-ef358450f3ac
					// https://support.microsoft.com/en-us/topic/you-may-receive-error-code-0x80004005-or-other-error-codes-when-you-try-to-start-a-windows-xp-based-computer-a15f5b2f-642d-24ac-4912-1570a6bcedec
					// https://answers.microsoft.com/en-us/msoffice/forum/msoffice_onenote-mso_amobile-msoversion_other/error-code-80004005/ba567a2a-e037-40c3-9a5e-428030db6223

					// When HS20 is not supported, an exception is thrown with these values
					// We ignore the error, except if no SSID was configured (ignored in the stanza ealier)
					// 1206 == ManagedNativeWifi.Win32.NativeMethod.ERROR_BAD_PROFILE
					if (e.ErrorCode == -2147467259 || e.NativeErrorCode == 1206)
					{
						Debug.WriteLine("Win32Exception: ERROR_BAD_PROFILE");
						return;
					}

					// Observed by an institution in The Netherlands, via Paul Dekkers
					// 183 == ManagedNativeWifi.Win32.NativeMethod.ERROR_ALREADY_EXISTS
					if (e.ErrorCode == -2147467259 || e.NativeErrorCode == 183)
					{
						Debug.WriteLine("Win32Exception: ERROR_ALREADY_EXISTS");

						// TODO try removing and retry?
						return;
					}

					// Not ignored, so throw
					throw;
				}
			}
		}

		private ConfiguredWLANProfile InstallProfile(string profileName, string profileXml, bool hs20, bool forAllUsers)
		{
			// security type not required
			const string securityType = null; // TODO: document why

			// overwrites if profile already exists
			const bool overwrite = true;

			// https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
			if (!NativeWifi.SetProfile(
				InterfaceId,
				forAllUsers
					? ProfileType.AllUser
					: ProfileType.PerUser, // TODO: make this option work and set as default
				profileXml,
				securityType,
				overwrite)) throw new Exception(
					"Unable to install " + (hs20 ? "Passpoint " : "SSID ") + profileName
				);

			ConfiguredWLANProfile configuredWLANProfile = new ConfiguredWLANProfile(InterfaceId, profileName, hs20);
			PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
				.Add(configuredWLANProfile);
			return configuredWLANProfile;
		}

		/// <summary>
		/// Sets user data (credentials) for a network profile.
		/// </summary>
		/// <param name="profile">The profile to add user data to</param>
		/// <param name="userDataXml">The user data XML</param>
		/// <param name="forAllUsers">Install for all users or only the current user</param>
		/// <remarks><paramref name="forAllUsers"/>MUST be false for EAP-TLS, probably because the certificate is in the user store</remarks>
		public void InstallUserData(ConfiguredWLANProfile profile, string userDataXml, bool forAllUsers)
		{
			if (profile.InterfaceId != InterfaceId)
				throw new ArgumentException("Provided profile is not for the same interface as this network");

			if (NativeWifi.SetProfileEapXmlUserData(
				profile.InterfaceId,
				profile.ProfileName,
				forAllUsers // cannot work with profileUserTypeAllUSers and EAP-TLS, probably because the certificate is in the user store?
					? EapXmlType.AllUsers
					: EapXmlType.Default
					,
				userDataXml))
			{
				if (!profile.HasUserData) // ommit uneccesary writes
				{
					PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
						.Remove(profile)
						.Add(profile.WithUserDataSet());
				}
			} else {
				throw new Exception("Unable to install UserProfile " + profile.ProfileName);
			}
		}

		/// <summary>
		/// Attempts to delete any previously installed network profiles
		/// </summary>
		/// <returns>True if ANY profile was deleted succesfully</returns>
		/// <remarks>
		/// True does not mean all the profiles has been deleted. Check IsConfigured ot verify this.
		/// </remarks>
		public void RemoveInstalledProfiles()
		{
			Debug.WriteLine("Removing installed profiles on " + InterfaceId);
			foreach (var configuredProfile in PersistingStore.ConfiguredWLANProfiles.ToList())
			{
				if (configuredProfile.InterfaceId == InterfaceId)
				{
					// We explicitly ignore errors from NativeWifi,
					// as any failures probably mean that our info and the info in the OS is out of sync
					NativeWifi.DeleteProfile(InterfaceId, configuredProfile.ProfileName); // May return false
					PersistingStore.ConfiguredWLANProfiles = PersistingStore.ConfiguredWLANProfiles
						.Remove(configuredProfile);
				}
			}

			if (0 != PersistingStore.ConfiguredWLANProfiles.Count)
			{
				throw new Exception("Some WLAN profile could not be removed");
			}
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
			PruneStaleProfiles();

			// TODO: multiple profiles on a single interface creates duplicate work further down
			//       perhaps group by InterfaceId and have a list of ProfileName in each EduroamNetwork?
			var availableNetworks = (eapConfig == null ? Enumerable.Empty<AvailableNetworkPack>() : GetAllMatchingNetworkPacks(eapConfig.SSIDs))
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
			PruneStaleProfiles();

			// join configured profiles
			return GetAllNetworkProfilePackPairs()
				.Join(PersistingStore.ConfiguredWLANProfiles,
					networkPPack => (networkPPack.ppack.Name, networkPPack.ppack.Interface.Id),
					persitedProfile => (persitedProfile.ProfileName, persitedProfile.InterfaceId),
					(networkPPack, persitedProfile) =>
						new EduroamNetwork(networkPPack.network, networkPPack.ppack, persitedProfile))
				.OrderByDescending(network => network.IsAvailable);
		}

		/// <param name="eapConfig">EAP config</param>
		/// <returns>true if at least one network is available</returns>
		public static bool IsNetworkInRange(EapConfig eapConfig)
		{
			if (eapConfig == null)
			{
				throw new ArgumentNullException(nameof(eapConfig));
			}

			// NICE TO HAVE: some way to detect if any Hs2 hotspot is available if no matching ssid are found
			return IsNetworkInRange(eapConfig.SSIDs);
		}

		/// <param name="ssids">SSID to look for</param>
		/// <returns>true if at least one network is available</returns>
		public static bool IsNetworkInRange(IEnumerable<string> ssids)
		{
			return GetAllMatchingNetworkPacks(ssids).Any();
		}

		private static void PruneStaleProfiles()
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
				Debug.WriteLine("Removing stale profile from persisting store called {0} on interface {1}",
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
		public static bool IsWlanServiceApiAvailable()
		{
			try
			{
				NativeWifi.EnumerateInterfaces();
			}
			catch (TargetInvocationException ex) // we don't know why it gets wrapped
			{
				if (ex.GetBaseException().GetType().Name == "Win32Exception")
					if (ex.GetBaseException().Message == "MethodName: WlanOpenHandle, ErrorCode: 1062, ErrorMessage: The service has not been started.\r\n")
						return false;

				Debug.Print(ex.ToString());
				throw; // unknown
			}
			catch (Win32Exception ex) // in case it doesn't get wrapped in RELEASE
			{
				if (ex.NativeErrorCode == 1062) // ERROR_SERVICE_NOT_ACTIVE
					return false;

				Debug.Print(ex.ToString());
				throw; // unknown
			}
			return true;
		}


		/// <summary>
		/// Gets all available network packs with a profile configured
		/// </summary>
		/// <returns>Network packs</returns>
		private static IEnumerable<AvailableNetworkPack> GetAllAvailableNetworkPacksWithProfiles()
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return Enumerable.Empty<AvailableNetworkPack>();

			// TODO, maybe join in the profile pack?

			return NativeWifi.EnumerateAvailableNetworks()
				.Where(network => !string.IsNullOrEmpty(network.ProfileName));
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
		private static IEnumerable<(ProfilePack ppack, AvailableNetworkPack network)> GetAllNetworkProfilePackPairs()
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return Enumerable.Empty<(ProfilePack, AvailableNetworkPack)>();

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

			return availableProfileNetworks.Concat(unavailableProfiles);
		}

		/// <summary>
		/// Get all available networks matching the SSID.
		/// </summary>
		/// <param name="credentialApplicabilities"></param>
		/// <returns>Network packs</returns>
		private static IOrderedEnumerable<AvailableNetworkPack> GetAllMatchingNetworkPacks(
			IEnumerable<string> ssids)
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return Enumerable.Empty<AvailableNetworkPack>().OrderBy(_ => false);

			return NativeWifi.EnumerateAvailableNetworks()
				.Where(network => ssids.Contains(network.Ssid.ToString()))
				.OrderBy(network => string.IsNullOrEmpty(network.ProfileName));
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
