using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
	public partial class frm5 : Form
	{
		readonly frmParent frmParent;
		public frm5(frmParent parentInstance)
		{
			frmParent = parentInstance;
			InitializeComponent();
		}

		private async void frm5_Load(object sender, EventArgs e)
		{
			// displays loading information while attempt to connect
			lblStatus.Text = "Connecting...";
			pboStatus.Image = Properties.Resources.ajax_loader;

			bool connectSuccess;
			// tries to connect
			try
			{
				connectSuccess = await Task.Run(ConnectToEduroam.Connect);
			}
			catch (Exception)
			{
				// if an exception is thrown, connection has not succeeded
				connectSuccess = false;
			}

			if (connectSuccess)
			{
				lblStatus.Text = "You are now connected to eduroam.";
				pboStatus.Image = Properties.Resources.checkmark_16;
			}
			else
			{
				lblStatus.Text = "Connection to eduroam failed.";
				pboStatus.Image = Properties.Resources.x_mark_3_16;
				lblConnectFailed.Visible = true;
				ConnectToEduroam.RemoveProfile();
			}
		}


	}
}
