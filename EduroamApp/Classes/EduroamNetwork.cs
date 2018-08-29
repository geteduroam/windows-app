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
		// Properties
		public AvailableNetworkPack NetworkPack { get; }
		public string Ssid { get; }
		public Guid InterfaceId { get; } = Guid.Empty;

		// Constructor
		public EduroamNetwork()
		{
			NetworkPack = GetEduroamPack();
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
		public static AvailableNetworkPack GetEduroamPack()
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
