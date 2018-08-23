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
			// opens dialog to select EAP Config file
			string eapConfigPath = GetFileFromDialog("Select EAP Config file");
			// prints out filepath
			txtFilepath.Text = eapConfigPath;

			//string eapConfigString = File.ReadAllText(eapConfigPath);
		}

		/// <summary>
		/// Lets user select a file through an OpenFileDialog.
		/// </summary>
		/// <param name="dialogTitle">Title of the OpenFileDialog.</param>
		/// <returns>Path of selected file.</returns>
		public string GetFileFromDialog(string dialogTitle)
		{
			string filePath = null;

			OpenFileDialog fileDialog = new OpenFileDialog
			{
				// sets the initial directory of the open file dialog
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				// sets filter for file types that appear in open file dialog
				Filter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*",
				FilterIndex = 0,
				RestoreDirectory = true,
				Title = dialogTitle
			};

			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				filePath = fileDialog.FileName;
			}

			return filePath;
		}

		/// <summary>
		/// Checks if a config file has been selected, and if the filepath and type is valid.
		/// </summary>
		/// <returns>True if valid file, false if not.</returns>
		public bool ValidateFileSelection()
		{
			string filePath = txtFilepath.Text;

			if (string.IsNullOrEmpty(filePath))
			{
				MessageBox.Show("Please select a file.",
								"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			else if (!File.Exists(filePath))
			{
				MessageBox.Show("The specified file does not exist.",
								"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			else if (Path.GetExtension(filePath) != ".eap-config")
			{
				MessageBox.Show("The file type you chose is not supported.",
								"Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			return true;
		}

		public uint ConnectWithFile()
		{
			uint eapType = 0;
			string instId = null;

			// validates the selected config file
			if (ValidateFileSelection())
			{
				// gets content of config file
				string eapString = File.ReadAllText(txtFilepath.Text);

				try
				{
					// gets certificates and creates wireless profile
					eapType  = ConnectToEduroam.Setup(eapString);
					instId = ConnectToEduroam.GetInstId(eapString);
				}
				catch (ArgumentException argEx)
				{
					if (argEx.Message == "interfaceId")
					{
						MessageBox.Show("Could not establish a connection through your computer's wireless network interface. \n" +
										"Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.",
							"Network interface error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}

					//catch (Exception ex)
					//{
					//    MessageBox.Show(
					//        "The selected file is corrupted. Please select another file, or try another setup method.\n" +
					//        "Exception: " + ex.Message,
					//        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					//}
				}
			}

			// makes the institution Id accessible from parent form
			frmParent.LblInstText = instId;
			return eapType;

		}
	}
}
