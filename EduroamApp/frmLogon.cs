using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;

namespace EduroamApp
{
	public partial class frmLogon : Form
	{
		// network pack from main form
		AvailableNetworkPack network;

		public frmLogon()
		{
			InitializeComponent();
		}

		public void GetEduroamInstance(AvailableNetworkPack existingNetwork)
		{
			// get network pack from main form
			network = existingNetwork;
		}

		private void frmLogon_Load(object sender, EventArgs e)
		{
			// clears the status label
			lblStatus.Text = "";
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			// gets username and password from UI
			string username = txtUsername.Text;
			string password = txtPassword.Text;

			// gets network pack info from previous form
			string ssid = network.Ssid.ToString();
			Guid interfaceId = network.Interface.Id;

			// generates user data xml file
			string userDataXml = UserDataXml.CreateUserDataXml(username, password);

			// sets user data
			SetUserData(interfaceId, ssid, userDataXml);

			// connects to eduroam
			var connectResult = Task.Run(() => ConnectAsync(network)).Result;
			lblStatus.Text = (connectResult ? "Connection success." : "Connection failed.");
		}


		/// <summary>
		/// Sets a profile's user data for login with username + password.
		/// </summary>
		/// <param name="networkID">Interface ID of selected network.</param>
		/// <param name="profileName">Name of associated wireless profile.</param>
		/// <param name="userDataXml">User data XML converted to string.</param>
		/// <returns>True if succeeded, false if failed.</returns>
		public static bool SetUserData(Guid networkID, string profileName, string userDataXml)
		{
			// sets the profile user type to "WLAN_SET_EAPHOST_DATA_ALL_USERS"
			uint profileUserType = 0x00000001;

			return NativeWifi.SetProfileUserData(networkID, profileName, profileUserType, userDataXml);
		}

		/// <summary>
		/// Connects to the chosen wireless LAN.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		public static async Task<bool> ConnectAsync(AvailableNetworkPack chosenWifi)
		{
			if (chosenWifi == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: chosenWifi.Interface.Id,
				profileName: chosenWifi.ProfileName,
				bssType: chosenWifi.BssType,
				timeout: TimeSpan.FromSeconds(10));
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
