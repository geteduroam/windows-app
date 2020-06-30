using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using ManagedNativeWifi;

namespace EduroamConfigure
{
	/// <summary>
	/// Connects to an Eduroam network if available and stores information about it.
	/// </summary>
	public class EduroamNetwork
	{
		public const string DefaultSsid = "eduroam";

		// Properties
		public Guid InterfaceId { get; }
		public AvailableNetworkPack NetworkPack { get; }
		public bool IsAvailable { get { return NetworkPack != null; } }
		public bool IsConfigured { get; private set; }

		// TODO: Add support for Hotspot2.0
		// TODO: Add support for Wired 801x

		private EduroamNetwork(Guid interfaceId)
		{
			NetworkPack = null;
			InterfaceId = interfaceId;
		}

		private EduroamNetwork(AvailableNetworkPack networkPack)
			: this(networkPack.Interface.Id)
		{
			NetworkPack = networkPack;
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
		/// Installs a network profile according to selected network and profile XML.
		/// Will overwrite if existing
		/// </summary>
		/// <param name="profileXml">User data XML converted to string.</param>
		/// <param name="forAllUsers">TODO</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public bool InstallProfile(string profileXml, bool forAllUsers = true)
		{
			// security type not required
			const string securityType = null; // TODO: document why

			// overwrites if profile already exists
			const bool overwrite = true;

			// https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
			return NativeWifi.SetProfile(
				InterfaceId,
				forAllUsers
					? ProfileType.AllUser
					: ProfileType.PerUser, // TODO: make this option work and set as default
				profileXml,
				securityType,
				overwrite);
		}

		/// <summary>
		/// Sets user data (credentials) for a network profile.
		/// </summary>
		/// <param name="userDataXml">User data XML converted to string.</param>
		/// <param name="forAllUsers">TODO - mention the cert store thing</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public bool InstallUserData(string userDataXml, bool forAllUsers = false)
		{
			// See 'dwFlags' at: https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofileeapxmluserdata
			const uint profileUserTypeCurrentUsers = 0x00000000; // "current user" - https://github.com/rozmansi/WLANSetEAPUserData
			const uint profileUserTypeAllUSers = 0x00000001; // WLAN_SET_EAPHOST_DATA_ALL_USERS


			var profileName = "eduroam"; // TODO!!!
			bool success = NativeWifi.SetProfileUserData(
				InterfaceId,
				profileName,
				forAllUsers
					? profileUserTypeAllUSers
					: profileUserTypeCurrentUsers,
				userDataXml);
			if (success) IsConfigured = true;
			return success;
		}

		/// <summary>
		/// Deletes a network profile by matching ssid on specified network interface
		/// </summary>
		/// <returns>True if profile delete was succesful</returns>
		public bool RemoveProfile()
		{
			return NativeWifi.DeleteProfile(InterfaceId, DefaultSsid);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public bool Cleanup() // TODO: use this
		{
			if (!IsConfigured) return false;
			RemoveProfile();
			IsConfigured = false;
			return true;
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
				timeout: TimeSpan.FromSeconds(5));
		}

		// static interface:

		/// <summary>
		/// Enumerates EduroamNetwork objects for all wireless network interfaces.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<EduroamNetwork> GetAll()
		{
			// NativeWifi will throw if service is not available
			if (!IsWlanServiceApiAvailable())
				return Enumerable.Empty<EduroamNetwork>();

			var availableNetworks = GetAllAvailableEduroamNetworkPacks()
				.Select(networkPack => new EduroamNetwork(networkPack))
				.ToList();

			List<Guid> configuredInterfaces = availableNetworks
				.Select(network => network.NetworkPack.Interface.Id)
				.ToList();

			// These are not available, but they are configurable
			var unavailableNetworks = GetAllInterfaceIds()
				.Where(guid => !configuredInterfaces.Contains(guid))
				.Select(guid => new EduroamNetwork(guid));

			return availableNetworks.Concat(unavailableNetworks);
		}


		/// <summary>
		/// "Can i install and connect to eduroam?"
		/// </summary>
		/// <returns></returns>
		public static bool IsEduroamAvailable(string ssid = DefaultSsid, string ConsortiumOid = null)
		{
			return GetAllAvailableEduroamNetworkPacks(ssid, ConsortiumOid).Any();
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
		/// Gets all network packs containing information about an eduroam network, if any.
		/// </summary>
		/// <returns>Network packs</returns>
		private static List<AvailableNetworkPack> GetAllAvailableEduroamNetworkPacks(string ssid = DefaultSsid, string ConsortiumOid = null)
		{
			if (!IsWlanServiceApiAvailable()) // NativeWifi.EnumerateAvailableNetworks will throw
				return new List<AvailableNetworkPack>();

			return NativeWifi.EnumerateAvailableNetworks()
				.Where(network => network.Ssid.ToString() == ssid) // TODO: ConsortiumOid
				.OrderBy(network => string.IsNullOrEmpty(network.ProfileName))
				.ToList();
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
