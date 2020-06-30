using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using ManagedNativeWifi;

namespace EduroamConfigure
{
    /// <summary>
    /// Connects to an Eduroam network if available and stores information about it.
    /// </summary>
    public class EduroamNetwork
    {
        public const string Ssid = "eduroam";

        // Properties
        public AvailableNetworkPack NetworkPack { get; }
        public bool IsAvailable { get; }
        public Guid InterfaceId { get; }

        // TODO: Add support for Hotspot2.0
        // TODO: Add support for Wired 801x

        private EduroamNetwork(Guid interfaceId)
        {
            NetworkPack = null;
            IsAvailable = false;
            InterfaceId = interfaceId;
        }

        private EduroamNetwork(AvailableNetworkPack networkPack)
            : this(networkPack.Interface.Id)
        {
            NetworkPack = networkPack;
            IsAvailable = true;
        }

        /// <summary>
        /// Enumerates EduroamNetwork objects for all wireless network interfaces.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EduroamNetwork> EnumerateEduroamNetworks()
        {
            return EnumerateAvailableEduroamNetworks()
                .Concat(EnumerateUnavailableEduroamNetworks());
        }

        /// <summary>
        /// Enumerates EduroamNetwork objects for wireless interfaces where is eduroam available
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EduroamNetwork> EnumerateAvailableEduroamNetworks()
        {
            return GetAllEduroamPacks().Select(networkPack => new EduroamNetwork(networkPack));
        }

        /// <summary>
        /// Enumerates EduroamNetwork objects for wireless interfaces with no eduroam available.
        /// Make profile creation still possible.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EduroamNetwork> EnumerateUnavailableEduroamNetworks()
        {
            List<Guid> configuredInterfaces = GetAllEduroamPacks()
                .Select(networkPack => networkPack.Interface.Id)
                .ToList();
            
            return GetAllInterfaceIds()
                .Where(guid => !configuredInterfaces.Contains(guid))
                .Select(guid => new EduroamNetwork(guid));
        }

        /// <summary>
        /// Tries to access the wireless interfaces and reports wether the service is available or not
        /// If this returns false, then no interfaces nor packs will be available to configure
        /// </summary>
        /// <returns>True if wireless service is available</returns>
        private static bool IsWlanServiceAvailable()
        {
            try
            {
                NativeWifi.EnumerateInterfaces().ToList();
            }
            catch (TargetInvocationException ex)
            {
                if (ex.GetBaseException().GetType().Name == "Win32Exception")
                    if (ex.GetBaseException().Message == "MethodName: WlanOpenHandle, ErrorCode: 1062, ErrorMessage: The service has not been started.\r\n")
                        return false;
                throw;
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1062) // ERROR_SERVICE_NOT_ACTIVE
                    return false;
                throw;
            }
            return true;
        }

        /// <summary>
        /// If any eduroam networks are available
        /// </summary>
        /// <returns></returns>
        public static bool IsEduroamAvailable() 
        {
            return GetAllEduroamPacks().Any();
        }

        /// <summary>
        /// Gets all network packs containing information about an eduroam network, if any.
        /// </summary>
        /// <returns>Network packs</returns>
        public static List<AvailableNetworkPack> GetAllEduroamPacks()
        {
            if (!IsWlanServiceAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
                return new List<AvailableNetworkPack>();

            return NativeWifi.EnumerateAvailableNetworks()
                .Where(network => network.Ssid.ToString() == Ssid)
                .OrderBy(network => string.IsNullOrEmpty(network.ProfileName))
                .ToList();
        }

        /// <summary>
        /// Gets the computer's wireless network interface Ids, if they exists.
        /// </summary>
        /// <returns>all Wireless interface IDs</returns>
        public static IEnumerable<Guid> GetAllInterfaceIds()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in interfaces)
            {
                // searches for wireless network interface
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.Speed != -1)
                {
                    yield return new Guid(nic.Id);
                }
            }
        }
    }
}
