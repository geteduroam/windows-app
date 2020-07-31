using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using EduroamConfigure;

namespace EduroamApp
{
	/// <summary>
	/// Shows status for connection to eduroam.
	/// Lets user save configuration for later.
	/// </summary>
	public partial class frmConnect : Form
	{
		private readonly frmParent frmParent;

		public frmConnect(frmParent parentInstance)
		{
			frmParent = parentInstance;
			InitializeComponent();
		}

		private void frmConnect_Load(object sender, EventArgs e)
		{
			if (frmParent.EduroamAvailable)
			{
				// connect if eduroam is available
				Connect();
			}
			else
			{
				// prompt user to save config if not
				SaveAndQuit();
			}
		}

		private async void Connect()
		{
			// displays loading animation while attempt to connect
			lblStatus.Text = "Connecting...";
			pbxStatus.Image = Properties.Resources.loading_gif;
			lblStatus.Visible = true;
			pbxStatus.Visible = true;

			if (frmParent.AuthMethod.EapType == EapType.TLS)
			{
				DateTime validFrom = frmParent.AuthMethod.ClientCertificateAsX509Certificate2().NotBefore;
				DateTime now = DateTime.Now;
				TimeSpan difference = validFrom - now;

				// if certificate valid from time has passed, do nothing
				if (DateTime.Compare(validFrom, now) > 0)
				{
					// waits at connecting screen if under 9 seconds difference
					if (difference.TotalSeconds < 8) // TODO: muyto intressante, add this to TryToConnect in ConnectToEduroam
					{
						await Task.Delay(difference.Milliseconds + 1000);
					}
					// displays dialog that lets user know how long they must wait, or to change their clock manually
					else
					{
						// opens form as dialog
						using frmSetTime setTimeDialog = new frmSetTime(validFrom);
						var dialogResult = setTimeDialog.ShowDialog();
						// cancels connection if time not set and dialog cancelled
						if (dialogResult == DialogResult.Cancel)
						{
							lblStatus.Text = "Couldn't connect to eduroam.";
							pbxStatus.Image = Properties.Resources.red_x;
							lblConnectFailed.Text =
								"Please ensure that the date time and time zone on your computer are set correctly.\n\n" +
								lblConnectFailed.Text;
							lblConnectFailed.Visible = true;
							frmParent.BtnBackEnabled = true;
							frmParent.ProfileCondition = frmParent.ProfileStatus.Incomplete;
							return;
						}
					}
				}
			}

			bool eduConnected = await Task.Run(frmParent.Connect);

			if (eduConnected)
			{
				lblStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
				pbxStatus.Image = Properties.Resources.green_checkmark;
				frmParent.BtnNextText = "Close";
				frmParent.BtnNextEnabled = true;
				frmParent.BtnBackVisible = false;
				frmParent.ProfileCondition = frmParent.ProfileStatus.Working;
			}
			else
			{
				lblStatus.Text = "Connection to eduroam failed.";
				pbxStatus.Image = Properties.Resources.red_x;
				lblConnectFailed.Visible = true;
				frmParent.BtnBackEnabled = true;
				frmParent.ProfileCondition = frmParent.ProfileStatus.Incomplete;
			}
		}

		// gives user choice of wether they want to save the configuration before quitting
		private void SaveAndQuit()
		{
			frmParent.ProfileCondition = frmParent.ProfileStatus.Working; // TODO: what?
			pnlEduNotAvail.Visible = true;
		}


	}
}
