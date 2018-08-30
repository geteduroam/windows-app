using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
	/// <summary>
	/// This
	/// </summary>
	public partial class frmConnect : Form
	{
		readonly frmParent frmParent;
		public frmConnect(frmParent parentInstance)
		{
			frmParent = parentInstance;
			InitializeComponent();
		}

		private async void frmConnect_Load(object sender, EventArgs e)
		{
			// displays loading information while attempt to connect
			lblStatus.Text = "Connecting...";
			pboStatus.Image = Properties.Resources.ajax_loader;

			bool connectSuccess;
			// tries to connect
			try
			{
				connectSuccess = await Task.Run(ConnectToEduroam.WaitForConnect);
			}

			catch (Exception ex)
			{
				// if an exception is thrown, connection has not succeeded
				connectSuccess = false;
				MessageBox.Show("Could not connect. \nException: " + ex.Message);
			}

			if (connectSuccess)
			{
				lblStatus.Text = "You are now connected to eduroam.\n\nPress Close to exit the wizard.";
				pboStatus.Image = Properties.Resources.checkmark_16;
				frmParent.BtnCancelText = "Close";
				frmParent.ProfileCondition = "GOODPROFILE";
			}
			else
			{
				lblStatus.Text = "Connection to eduroam failed.";
				pboStatus.Image = Properties.Resources.x_mark_3_16;
				lblConnectFailed.Visible = true;
				frmParent.BtnBackEnabled = true;
				frmParent.ProfileCondition = "BADPROFILE";
			}
		}


	}
}
