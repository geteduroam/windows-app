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
		public AvailableNetworkPack networkPack { get; } = null;
		public string ssid { get; } = null;
		public Guid interfaceId { get; } = Guid.Empty;

		/// <summary>
		/// Constructor
		/// </summary>
		public EduroamNetwork()
		{
			networkPack = GetEduroam();
			if (networkPack != null) // only assigns value to properties if network pack exists
			{
				ssid = networkPack.Ssid.ToString();
				interfaceId = networkPack.Interface.Id;
			}
		}

		/// <summary>
		/// Gets a network pack containing information about an eduroam network, if available.
		/// </summary>
		/// <returns>Network pack.</returns>
		private AvailableNetworkPack GetEduroam()
		{
			// gets all available networks and stores them in a list
			List<AvailableNetworkPack> networks = NativeWifi.EnumerateAvailableNetworks().ToList();

			// sets eduroam as the chosen network,
			// prefers a network with an existing profile
			foreach (AvailableNetworkPack network in networks)
			{
				if (network.Ssid.ToString() == "eduroam")
				{
					foreach (AvailableNetworkPack network2 in networks)
					{
						if (network2.Ssid.ToString() == "eduroam" && network2.ProfileName != "")
						{
							return network2;
						}
					}
					return network;
				}
			}
			// if no networks called "eduroam" are found, return nothing
			return null;
		}
	}
}
