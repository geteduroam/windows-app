using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedNativeWifi;

namespace EduroamApp
{
    class EduroamNetwork
    {
        /// <summary>
        ///Properties
        /// </summary>
        public AvailableNetworkPack NetworkPack { get; } = null;
        public string Ssid { get; } = null;
        public Guid InterfaceId { get; } = Guid.Empty;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EduroamNetwork()
        {
            NetworkPack = GetEduroam();
            if (NetworkPack != null) // only assigns value to properties if network pack exists
            {
                Ssid = NetworkPack.Ssid.ToString();
                InterfaceId = NetworkPack.Interface.Id;
            }            
        }

        /// <summary>
        /// Gets a network pack containing information about an eduroam network, if available.
        /// </summary>
        /// <returns>Network pack.</returns>
        public static AvailableNetworkPack GetEduroam()
        {
            // gets all available networks and stores them in a list
            List<AvailableNetworkPack> networks = NativeWifi.EnumerateAvailableNetworks().ToList();

            // gets eduroam network pack, prefers a network with an existing profile
            foreach (AvailableNetworkPack network in networks)
            {
                if (network.Ssid.ToString() == "eduroam" && network.ProfileName != "")
                {
                    return network;
                }
            }
            // if no profiles exist for eduroam, search again and get network pack without profile
            foreach (AvailableNetworkPack network in networks)
            {
                if (network.Ssid.ToString() == "eduroam")
                {
                    return network;
                }
            }
            // if no networks called "eduroam" are found, return nothing
            return null;
        }
    }
}
