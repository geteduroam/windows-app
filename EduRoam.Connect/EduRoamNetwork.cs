using EduRoam.Connect.Eap;
using EduRoam.Connect.Store;
using EduRoam.Localization;

using ManagedNativeWifi;

using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;

using WLANProfile = EduRoam.Connect.Store.WLANProfile;

namespace EduRoam.Connect
{
    /// <summary>
    /// Connects to an Eduroam network if available and stores information about it.
    /// Note: this struct is read only. After using it to store changes, fetch the networks again to see the changes.
    /// </summary>
    public readonly struct EduRoamNetwork
    {
        // TODO: Add support for Wired 801x

        // Properties
        private readonly BaseConfigStore store = new RegistryStore();

        private AvailableNetworkPack? NetworkPack { get; }

        private ProfilePack? ProfilePack { get; }

        public Guid InterfaceId { get; }

        public string ProfileName { get => this.ProfilePack?.Name ?? this.NetworkPack?.ProfileName ?? ""; }

        public bool IsAvailable { get => this.NetworkPack != null; }

        private EduRoamNetwork(Guid interfaceId)
        {
            this.NetworkPack = null;
            this.ProfilePack = null;
            this.InterfaceId = interfaceId; // non-nullable
        }

        private EduRoamNetwork(
            AvailableNetworkPack networkPack,
            ProfilePack? profilePack,
            object? _ = null) // last argument is ConfiguredWLANProfile
            : this(profilePack?.Interface.Id ?? networkPack.Interface.Id)
        {
            Debug.Assert((networkPack, profilePack) != (null, null));
            Debug.Assert(
                networkPack == null
                || profilePack == null
                || networkPack.Interface.Id == profilePack.Interface.Id);

            this.NetworkPack = networkPack;
            this.ProfilePack = profilePack;
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
        public static bool IsAuthMethodSupported(Eap.AuthenticationMethod authMethod)
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
        /// <param name="authMethod">AuthMethod of profiles to be installed</param>
        /// <param name="forAllUsers">Install for all users or only the current user</param>
        /// <returns>(success with ssid, success with hotspot2)</returns>
        /// <remarks><paramref name="forAllUsers"/> is ignored because it currently must be true for profiles and false for eapxml</remarks>
        public void InstallProfiles(Eap.AuthenticationMethod authMethod, bool forAllUsers = true)
        {
            _ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));

            RegistryStore.Instance.UpdateIdentity(authMethod.ClientUserName, IdentityProviderInfo.From(authMethod));

            var ssids = authMethod.SSIDs;

            WLANProfile profile;
            foreach (var ssid in ssids)
            {
                (var profileName, var profileXml) = ProfileXml.CreateSSIDProfileXml(authMethod, ssid);
                var userDataXml = UserDataXml.CreateUserDataXml(authMethod);
                try
                {
                    // forAllUsers must be true when installing the profile, but false when installing userdata
                    // Otherwise the profile is installed but doens't work.  We don't know why.
                    profile = this.InstallProfile(profileName, profileXml, hs20: false, forAllUsers: true);
                    // forAllUsers does not work with EAP-TLS, probably because the certificate is in the personal store
                    // Same for all methods where the CA is not public
                    this.InstallUserData(profile, userDataXml, false);
                }
                catch (Win32Exception e)
                {
                    Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                    Debug.Print(e.ToString());
                    Debug.Assert(false);

                    throw;
                }
            }

            if (authMethod.IsHS20Supported)
            {
                (var profileName, var profileXml) = ProfileXml.CreateHS20ProfileXml(authMethod);
                var userDataXml = UserDataXml.CreateUserDataXml(authMethod);
                try
                {
                    // forAllUsers must be true when installing the profile, but false when installing userdata
                    // Otherwise the profile is installed but doens't work.  We don't know why.
                    profile = this.InstallProfile(profileName, profileXml, hs20: true, forAllUsers: true);
                    // forAllUsers does not work with EAP-TLS, probably because the certificate is in the personal store
                    // Same for all methods where the CA is not public
                    this.InstallUserData(profile, userDataXml, forAllUsers: false);
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
#if !DEBUG
					}
					else
					{
						// Accept any error when ssids.Count > 0 for Release version.
						// We still are not sure what kind of errors are to be expected
						// when configuring HS20, so this gives the best user experience
						return;
#endif
                    }

                    // -2147467259 == 0x80004005, which is the most generic error code Windows has to offer
                    // Useless but fun reading:
                    // https://support.microsoft.com/en-us/windows/fix-error-0x80004005-9acfca89-b5e4-b976-6fa1-ef358450f3ac
                    // https://support.microsoft.com/en-us/topic/you-may-receive-error-code-0x80004005-or-other-error-codes-when-you-try-to-start-a-windows-xp-based-computer-a15f5b2f-642d-24ac-4912-1570a6bcedec
                    // https://answers.microsoft.com/en-us/msoffice/forum/msoffice_onenote-mso_amobile-msoversion_other/error-code-80004005/ba567a2a-e037-40c3-9a5e-428030db6223

                    // When HS20 is not supported, an exception is thrown with these values
                    // We ignore the error, except if no SSID was configured (ignored in the stanza ealier)
                    // 1206 == ManagedNativeWifi.Win32.NativeMethod.ERROR_BAD_PROFILE
                    if (e.ErrorCode == -2147467259 && e.NativeErrorCode == 1206)
                    {
                        Debug.WriteLine("Win32Exception: ERROR_BAD_PROFILE");
                        return;
                    }

                    // Observed by an institution in The Netherlands, via Paul Dekkers
                    // 183 == ManagedNativeWifi.Win32.NativeMethod.ERROR_ALREADY_EXISTS
                    if (e.ErrorCode == -2147467259 && e.NativeErrorCode == 183)
                    {
                        Debug.WriteLine("Win32Exception: ERROR_ALREADY_EXISTS");

                        // TODO try removing and retry?
                        return;
                    }

                    // This error code happens when attempting to configure TTLS-EAP-MSCHAPv2 user data
                    // We don't really know why this happens; UserDataXml::IsSupported() will advise against
                    // using this authentication method.  If we got here anyway, it's a bug.
                    if (e.ErrorCode == -2147467259 && e.NativeErrorCode == 0xE225) /* 57893 */
                    {
                        Debug.WriteLine("Win32Exception: NativeErrorCode 0xE225 (57893), TTLS-EAP-MSCHAPv2 attempted? - THIS SHOULD NOT HAPPEN");
                        throw;
                    }

                    Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                    Debug.Assert(false);

                    // Not ignored, so throw
                    throw;
                }
            }
        }

        private WLANProfile InstallProfile(string profileName, string profileXml, bool hs20, bool forAllUsers)
        {
            // security type not required
            const string? SecurityType = null; // TODO: document why

            // overwrites if profile already exists
            const bool Overwrite = true;

            // https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
            if (!NativeWifi.SetProfile(
                    this.InterfaceId,
                    forAllUsers
                        ? ProfileType.AllUser
                        : ProfileType.PerUser, // TODO: make this option work and set as default
                    profileXml,
                    SecurityType,
                    Overwrite))
            {
                throw new Exception(
                    "Unable to install " + (hs20 ? "Passpoint " : "SSID ") + profileName
                );
            }

            var configuredWLANProfile = new WLANProfile(this.InterfaceId, profileName, hs20);
            this.store.AddConfiguredWLANProfile(configuredWLANProfile);
            return configuredWLANProfile;
        }

        /// <summary>
        /// Sets user data (credentials) for a network profile.
        /// </summary>
        /// <param name="profile">The profile to add user data to</param>
        /// <param name="userDataXml">The user data XML</param>
        /// <param name="forAllUsers">Install for all users or only the current user</param>
        /// <remarks><paramref name="forAllUsers"/>MUST be false for EAP-TLS, probably because the certificate is in the user store</remarks>
        public void InstallUserData(WLANProfile profile, string userDataXml, bool forAllUsers)
        {
            if (profile.InterfaceId != this.InterfaceId)
            {
                throw new ArgumentException("Provided profile is not for the same interface as this network");
            }

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
                    this.store.RemoveConfiguredWLANProfile(profile);
                    this.store.AddConfiguredWLANProfile(profile.WithUserDataSet());
                }
            }
            else
            {
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
            Debug.WriteLine("Removing installed profiles on " + this.InterfaceId);
            foreach (var configuredProfile in this.store.ConfiguredWLANProfiles.ToList())
            {
                if (configuredProfile.InterfaceId == this.InterfaceId)
                {
                    // We explicitly ignore errors from NativeWifi,
                    // as any failures probably mean that our info and the info in the OS is out of sync
                    NativeWifi.DeleteProfile(this.InterfaceId, configuredProfile.ProfileName); // May return false
                    this.store.RemoveConfiguredWLANProfile(configuredProfile);
                }
            }

            if (this.store.ConfiguredWLANProfiles.Any())
            {
                throw new Exception(Resources.ErrorCannotRemoveWLANProfile);
            }
        }

        public async Task<bool> TryToConnect()
        {
            if (!this.IsAvailable)
            {
                return false;
            }

            return await NativeWifi.ConnectNetworkAsync(
                interfaceId: this.NetworkPack.Interface.Id,
                profileName: this.NetworkPack.ProfileName,
                bssType: this.NetworkPack.BssType,
                timeout: TimeSpan.FromSeconds(8));
        }

        // static interface:

        /// <summary>
        /// Enumerates EduroamNetwork objects for all wireless network interfaces.
        /// </summary>
        /// <param name="eapConfig">Optional eap config to search with.</param>
        public static IEnumerable<EduRoamNetwork> GetAll(EapConfig? eapConfig = null)
        {
            // NativeWifi will throw if service is not available
            if (!IsWlanServiceApiAvailable())
            {
                return Enumerable.Empty<EduRoamNetwork>();
            }

            PruneStaleProfiles();

            // TODO: multiple profiles on a single interface creates duplicate work further down
            //       perhaps group by InterfaceId and have a list of ProfileName in each EduroamNetwork?
            var availableNetworks = (eapConfig == null ? Enumerable.Empty<AvailableNetworkPack>() : GetAllMatchingNetworkPacks(eapConfig.SSIDs))
                .Select(networkPack => new EduRoamNetwork(networkPack, null))
                .ToList();

            var configuredInterfaces = availableNetworks
                .Select(network => network.NetworkPack.Interface.Id)
                .ToImmutableHashSet();

            // These are not available, but they are configurable
            var unavailableNetworks = GetAllInterfaceIds()
                .Where(guid => !configuredInterfaces.Contains(guid))
                .Select(guid => new EduRoamNetwork(guid));

            return availableNetworks.Concat(unavailableNetworks);
        }

        /// <summary>
        /// Yields a EduroamNetwork instance for each configured profile for each network interface.
        /// </summary>
        public static IEnumerable<EduRoamNetwork> GetConfigured()
        {
            // NativeWifi will throw if service is not available
            if (!IsWlanServiceApiAvailable())
            {
                return Enumerable.Empty<EduRoamNetwork>();
            }

            PruneStaleProfiles();

            // join configured profiles
            return GetAllNetworkProfilePackPairs()
                .Join(inner: RegistryStore.Instance.ConfiguredWLANProfiles,
                    outerKeySelector: networkPPack => (networkPPack.ppack.Name, networkPPack.ppack.Interface.Id),
                    innerKeySelector: persitedProfile => (persitedProfile.ProfileName, persitedProfile.InterfaceId),
                    // note, EduroamNetwork doesn't actually use persistedProfile
                    resultSelector: (networkPPack, persistedProfile) => new EduRoamNetwork(networkPPack.network, networkPPack.ppack, persistedProfile))
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
            foreach (var configuredProfile in RegistryStore.Instance.ConfiguredWLANProfiles)
            {
                // if still installed
                if (installedProfiles.Any(ppack
                        => configuredProfile.InterfaceId == ppack.Interface.Id
                        && configuredProfile.ProfileName == ppack.Name))
                {
                    continue; // ignore
                }

                // else remove
                Debug.WriteLine("Removing stale profile from persisting store called {0} on interface {1}",
                    configuredProfile.ProfileName, configuredProfile.InterfaceId);
                RegistryStore.Instance.RemoveConfiguredWLANProfile(configuredProfile);
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
                {
                    if (ex.GetBaseException().Message == "MethodName: WlanOpenHandle, ErrorCode: 1062, ErrorMessage: The service has not been started.\r\n")
                    {
                        return false;
                    }
                }

                if (ex.GetBaseException().GetType().Name == "DllNotFoundException")
                {
                    if (ex.GetBaseException().HResult == -2146233052) // Message: Unable to load DLL 'Wlanapi.dll': The specified module could not be found. (Exception from HRESULT: 0x8007007E))
                    {
                        return false;
                    }
                }

                Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                Debug.Print(ex.ToString());
                Debug.Assert(false);
                throw; // unknown
            }
            catch (Win32Exception ex) // in case it doesn't get wrapped in RELEASE
            {
                if (ex.NativeErrorCode == 1062) // ERROR_SERVICE_NOT_ACTIVE
                {
                    return false;
                }

                Debug.WriteLine("THIS SHOULD NOT HAPPEN");
                Debug.Print(ex.ToString());
                Debug.Assert(false);
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
            {
                return Enumerable.Empty<AvailableNetworkPack>();
            }

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
            {
                return Enumerable.Empty<ProfilePack>();
            }

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
            {
                return Enumerable.Empty<(ProfilePack, AvailableNetworkPack)>();
            }

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
