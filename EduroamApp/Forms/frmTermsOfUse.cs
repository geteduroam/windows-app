using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp.Forms
{
	public partial class frmTermsOfUse : Form
	{
		private readonly string tou;

		public frmTermsOfUse(string termsOfUse)
		{
			tou = termsOfUse;
			InitializeComponent();
		}

		private void frmTermsOfUse_Load(object sender, EventArgs e)
		{
			txtToU.Text = tou;
		}
	}
}
