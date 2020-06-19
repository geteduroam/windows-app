using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
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
		/// Tries to access the wireless interfaces and reports wether the service is available or not
		/// </summary>
		/// <returns>True if wireless service is available</returns>
		public static bool IsWlanServiceAvailable()
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
				throw ex;
			}
			catch (Win32Exception ex)
			{
				if (ex.NativeErrorCode == 1062) // ERROR_SERVICE_NOT_ACTIVE
					return false;
				throw ex;
			}
			return true;
		}

		/// <summary>
		/// Gets a network pack containing information about an eduroam network, if available.
		/// </summary>
		/// <returns>Network pack or null</returns>
		public static AvailableNetworkPack GetEduroamPack()
		{
			if (!IsWlanServiceAvailable()) return null;

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
