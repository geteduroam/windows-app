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
	public partial class frmWaitForAuthenticate : Form
	{
		public frmWaitForAuthenticate()
		{
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			// WebServer.CancelListener();
		}

		public System.Windows.Forms.Button BtnCancel => btnCancel;

		private void frmWaitForAuthenticate_Load(object sender, EventArgs e)
		{

		}
	}
}
