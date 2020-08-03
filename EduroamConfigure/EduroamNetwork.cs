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
using ConfiguredProfile = EduroamConfigure.PersistingStore.ConfiguredProfile;

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
        public Guid InterfaceId { get; }
        public AvailableNetworkPack NetworkPack { get; }
        public bool IsAvailable { get => NetworkPack != null; }
        public ConfiguredProfile? Profile { get; }

        // TODO: Add support for Wired 801x

        private EduroamNetwork(Guid interfaceId)
        {
            NetworkPack = null;
            Profile = null;
            InterfaceId = interfaceId;
        }

        private EduroamNetwork(AvailableNetworkPack networkPack, ConfiguredProfile? profile = null)
            : this(networkPack.Interface.Id)
        {
            NetworkPack = networkPack;
            Profile = profile;

            /*
            if (!string.IsNullOrEmpty(networkPack.ProfileName))
            {
                ProfilePack profilePack = NativeWifi.EnumerateProfiles()
                   .Where(pp => pp.Interface.Id == networkPack.Interface.Id)
                   .First(pp => pp.Name == networkPack.ProfileName);

                profilePack.Document.Xml
            }
            */
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

            bool installed = false;
            foreach (var ssid in ssids)
            {
                (string profileName, string profileXml) = ProfileXml.CreateProfileXml(authMethod, ssid);
                installed |= InstallProfile(profileName, profileXml, false, forAllUsers);
            }

            bool installedHs2 = false;
            if (authMethod.Hs2AuthMethod != null)
                installedHs2 = InstallHs2Profile(authMethod.Hs2AuthMethod, forAllUsers);

            return (installed, installedHs2);
        }

        /// <summary>
        /// Installs a Hotspot 2.0 network profile according to selected auth method.
        /// Auth method must support Hotspot 2.0.
        /// Will overwrite any profiles with matching names if they exist.
        /// </summary>
        /// <param name="authMethod">TODO</param>
        /// <param name="forAllUsers">TODO</param>
        /// <returns>True if succeeded, false if failed.</returns>
        private bool InstallHs2Profile(EapConfig.AuthenticationMethod authMethod, bool forAllUsers = true)
        {
            _ = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));

            PersistingStore.ProfileID = authMethod.EapConfig.Uid;
            
            (string profileName, string profileXml) = ProfileXml.CreateProfileXml(authMethod, asHs2Profile: true);
            return InstallProfile(profileName, profileXml, true, forAllUsers);
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
                PersistingStore.ConfiguredProfiles = PersistingStore.ConfiguredProfiles
                    .Add(new ConfiguredProfile(InterfaceId, profileName, isHs2));
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

            bool ret = PersistingStore.ConfiguredProfiles.Any();
            foreach (var configuredProfile in PersistingStore.ConfiguredProfiles.ToList())
            {
                if (configuredProfile.InterfaceId != InterfaceId) continue;

                // generate user data xml file
                string userDataXml = UserDataXml.CreateUserDataXml(
                    configuredProfile.IsHs2
                        ? authMethod.Hs2AuthMethod
                        : authMethod, // TODO: move this logic into UserDataXml?
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

                Debug.WriteLine("Installed {2}UserProfile {3} for '{0}' on {1}",
                        configuredProfile.ProfileName, 
                        InterfaceId, 
                        configuredProfile.IsHs2 ? "Hs2 " : "", 
                        success ? "succeeded" : "failed");

                if (success && !configuredProfile.HasUserData)
                {
                    PersistingStore.ConfiguredProfiles = PersistingStore.ConfiguredProfiles
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
            var n = PersistingStore.ConfiguredProfiles.Count;

            Debug.WriteLine("Removing installed profiles on " + InterfaceId);
            foreach (var configuredProfile in PersistingStore.ConfiguredProfiles.ToList())
            {
                if (configuredProfile.InterfaceId == InterfaceId)
                {
                    if (NativeWifi.DeleteProfile(InterfaceId, configuredProfile.ProfileName))
                    {
                        PersistingStore.ConfiguredProfiles = PersistingStore.ConfiguredProfiles
                            .Remove(configuredProfile);
                    }
                }
            }

            return n != PersistingStore.ConfiguredProfiles.Count || n == 0;
        }

        public async Task<bool> TryToConnect()
        {
            // TODO: check if configured

            if (string.IsNullOrEmpty(NetworkPack.ProfileName))
                return false;

            // TODO: hotspot2.0 support ?

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
                .Select(networkPack => new EduroamNetwork(networkPack))
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
        /// Yields EduroamNetwork instances for each configured profile for each network interface.
        /// </summary>
        public static IEnumerable<EduroamNetwork> GetConfigured()
        {
            // NativeWifi will throw if service is not available
            if (!IsWlanServiceApiAvailable())
                return Enumerable.Empty<EduroamNetwork>();
            PruneMissingPersistedProfiles();
            
            return GetAllNetworkPacksWithProfiles()
                .Join(PersistingStore.ConfiguredProfiles,
                    network => (network.ProfileName, network.Interface.Id),
                    profile => (profile.ProfileName, profile.InterfaceId),
                    (network, profile) => new EduroamNetwork(network, profile));
        }

        /// <summary>
        /// "Can i install and connect to eduroam?"
        /// </summary>
        /// <param name="eapConfig">EAP config</param>
        /// <returns>true if eduroam is available</returns>
        public static bool IsEduroamAvailable(EapConfig eapConfig)
        {
            return GetAllAvailableEduroamNetworkPacks(eapConfig?.CredentialApplicabilities).Any();
        }

        private static void PruneMissingPersistedProfiles()
        {
            // look through installed profiles and remove persisted profile configurations which have been uninstalled by user
            // TODO: move to separate function?
            var availableProfiles = GetAllNetworkPacksWithProfiles().ToList();
            foreach (var configuredProfile in PersistingStore.ConfiguredProfiles)
            {
                // if still installed
                if (availableProfiles.Any(network
                        => configuredProfile.InterfaceId == network.Interface.Id
                        && configuredProfile.ProfileName == network.ProfileName))
                    continue; // ignore

                // else remove
                Debug.WriteLine("Removing profile from persisting store called {0} on interface {1}",
                    configuredProfile.ProfileName, configuredProfile.InterfaceId);
                PersistingStore.ConfiguredProfiles = PersistingStore.ConfiguredProfiles
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
        /// Gets all network packs containing information about an eduroam network, if any.
        /// </summary>
        /// <returns>Network packs</returns>
        private static List<AvailableNetworkPack> GetAllNetworkPacksWithProfiles()
        {
            if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
                return new List<AvailableNetworkPack>();

            return NativeWifi.EnumerateAvailableNetworks()
                .Where(network => !string.IsNullOrEmpty(network.ProfileName))
                .ToList();
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
                .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) // TODO: Wired 802.1x
                .Where(nic => nic.Speed != -1) // lol
                .Select(nic => new Guid(nic.Id));
        }

    }
}
