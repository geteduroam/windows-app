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
	public partial class frm2 : Form
	{
		// makes the parent form accessible from this class
		readonly frmParent frmParent;

		public frm2(frmParent parentInstance)
		{
			// gets the parent form instance
			frmParent = parentInstance;
			InitializeComponent();
		}

		/// <summary>
		/// Checks which radio button is selected and loads corresponding form.
		/// </summary>
		public void GoToForm()
		{
			if (rdbDownload.Checked)
			{
				// loads "Select insitute and download" form
				frmParent.LoadFrm3();
			}
			else if (rdbLocal.Checked)
			{
				// loads "Select local config file" form
				frmParent.LoadFrm4();
			}
		}

		private void rdbDownload_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void frm2_Load(object sender, EventArgs e)
		{

		}
	}
}
