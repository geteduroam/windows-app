using System;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EduroamApp
{
	/// <summary>
	/// Lets user select a local EAP-config file, or a local client certificate if required by EAP setup.
	/// </summary>
	public partial class frmLocal : Form
	{
		// makes parent form accessible from this class
		private readonly frmParent frmParent;

		public frmLocal(frmParent parentInstance)
		{
			// gets parent form instance
			frmParent = parentInstance;
			InitializeComponent();
		}

		// lets user browse their PC for a file
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			var dialogTitle = "";
			var dialogFilter = "";
			// expected filetype depends on label in parent form
			switch (frmParent.LocalFileType)
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

		/// <summary>
		/// Gets EAP file and creates an EapConfig object from it.
		/// </summary>
		/// <returns>EapConfig object.</returns>
		public EapConfig LocalEapConfig()
		{
			// validates the selected config file
			if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "EAP")) return null;

			// gets content of config file
			string eapString = File.ReadAllText(txtFilepath.Text);
			// creates and returns EapConfig object
			return ConnectToEduroam.GetEapConfig(eapString);
		}

		/// <summary>
		/// Installs a client certificate from file.
		/// </summary>
		/// <returns>True if cert installation success, false if not.</returns>
		public bool InstallCertFile()
		{
			// validates file selection
			if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "CERT")) return false;

			try
			{
				var certificate = new X509Certificate2(txtFilepath.Text, txtCertPassword.Text);
				return true;
			}
			// checks if correct password by trying to install certificate
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

		private void txtFilepath_TextChanged(object sender, EventArgs e)
		{
			// stops checking if expected file type is not client certificate
			if (frmParent.LocalFileType == "EAPCONFIG") return;

			// checks if password required by trying to install certificate
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

			// shows/hides password related controls
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
