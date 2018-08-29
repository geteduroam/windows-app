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
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Device.Location;

namespace EduroamApp
{
	public partial class frmLocal : Form
	{
		readonly frmParent frmParent;

		public frmLocal(frmParent parentInstance)
		{
			frmParent = parentInstance;
			InitializeComponent();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			string dialogTitle = "";
			string dialogFilter = "";
			switch (frmParent.LblLocalFileType)
			{
				case "EAPCONFIG":
					dialogTitle = "Select EAP Config file";
					dialogFilter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*";
					break;
				case "CERT":
					dialogTitle = "Select client certificate";
					dialogFilter = "Certificate files (*.PFX, *.P12)|*.pfx;*.p12|All files (*.*)|*.*";
					break;
			}
			// opens dialog to select file
			string selectedFilePath = FileDialog.GetFileFromDialog(dialogTitle, dialogFilter);
			// prints out filepath
			txtFilepath.Text = selectedFilePath;


		}

		public EapConfig ConnectWithFile()
		{
			// validates the selected config file
			if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "EAP")) return null;

			// gets content of config file
			string eapString = File.ReadAllText(txtFilepath.Text);
			// creates and returns EapConfig object
			return ConnectToEduroam.GetEapConfig(eapString);
		}

		public bool InstallCertFile()
		{
			if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "CERT")) return false;

			try
			{
				var certificate = new X509Certificate2(txtFilepath.Text, txtCertPassword.Text);
				return true;
			}
			catch (CryptographicException ex)
			{
				if ((ex.HResult & 0xFFFF) == 0x56)
				{
					MessageBox.Show("The password you entered is incorrect.", "Certificate install",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					MessageBox.Show("Could not install certificate.\nException: " + ex.Message, "Certificate install",
									 MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				return false;
			}
		}

		// checks wether certificate requires password or not
		private void txtFilepath_TextChanged(object sender, EventArgs e)
		{
			if (frmParent.LblLocalFileType == "EAPCONFIG") return;

			var passwordRequired = false;
			try
			{
				var testCertificate = new X509Certificate2(txtFilepath.Text, "");
			}
			catch (CryptographicException ex)
			{
				if ((ex.HResult & 0xFFFF) == 0x56)
				{
					passwordRequired = true;
				}
			}
			catch (Exception)
			{
				// ignored
			}

			if (passwordRequired)
			{
				lblCertPassword.Visible = true;
				txtCertPassword.Visible = true;
				chkShowPassword.Visible = true;
			}
			else
			{
				lblCertPassword.Visible = false;
				txtCertPassword.Visible = false;
				txtCertPassword.Text = "";
				chkShowPassword.Visible = false;
				chkShowPassword.Checked = false;
				txtCertPassword.UseSystemPasswordChar = true;
			}
		}

		// unmasks password characters on screen
		private void cboShowPassword_CheckedChanged(object sender, EventArgs e)
		{
			txtCertPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
		}
	}
}
