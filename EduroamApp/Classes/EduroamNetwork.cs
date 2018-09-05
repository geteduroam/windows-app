using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using ManagedNativeWifi;

namespace EduroamApp
{
	/// <summary>
	/// Connects to an Eduroam network if available and stores information about it.
	/// </summary>
	class EduroamNetwork
	{
		// Properties
		public AvailableNetworkPack NetworkPack { get; }
		public string Ssid { get; }
		public Guid InterfaceId { get; }

		// Constructor
		public EduroamNetwork()
		{
			NetworkPack = GetEduroamPack();
			// if eduroam network available, get ssid and interface id from network pack
			if (NetworkPack != null)
			{
				Ssid = NetworkPack.Ssid.ToString();
				InterfaceId = NetworkPack.Interface.Id;
			}
			// if eduroam network not available, hardcode ssid and get interface id so profile creation still possible
			else
			{
				Ssid = "eduroam";
				InterfaceId = GetInterfaceId();
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

		/// <summary>
		/// Gets the computer's wireless network interface Id, if it exists.
		/// </summary>
		/// <returns>Wireless interface id.</returns>
		public static Guid GetInterfaceId()
		{
			var interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface nic in interfaces)
			{
				// searches for wireless network interface
				if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.Speed != -1)
				{
					return new Guid(nic.Id);
				}
			}
			return Guid.Empty;
		}
	}
}
