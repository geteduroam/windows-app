using System.Drawing;
using System.Windows.Forms;
using System;

namespace EduroamApp
{
	/// <summary>
	/// This form lets the user select how they want to obtain an EAP-config file through a pair of radio buttons.
	/// Their options are as follows:
	/// - Select an institution and download config.
	/// - Browse local files for config.
	/// </summary>
	public partial class frmSelectMethod : Form
	{
		// makes parent form accessible from this class
		readonly frmParent frmParent;
		public bool newProfile { get; set; }

		public frmSelectMethod(frmParent parentInstance)
		{
			// gets parent form instance
			frmParent = parentInstance;
			newProfile = false;

			InitializeComponent();
		}

		private void frmSelectMethod_Load(object sender, EventArgs e)
		{
			if (frmParent.ComesFromSelfExtract)
			{
				btnExisting.Visible = true;
				btnExisting.Text = "Connect with \n" + frmParent.eapConfig.InstitutionInfo.DisplayName;
			}
			else
			{
				btnExisting.Visible = false;
			}

			// if no internet
			if (!frmParent.Online)
			{
				btnExisting.Enabled = false;
				btnNewProfile.Enabled = false;
				btnLocalProfile.Enabled = false;
				frmParent.TitleText = "Offline";
				label1.Text = "You seem to be offline or the Eduroam servers are unreachable. Please ensure that you have an internet connection and restart this application";
			}
		}


		private void btnNewProfile_Click(object sender, System.EventArgs e)
		{
			newProfile = true;
			frmParent.btnNext_Click(sender, e);
		}

		private void btnLocalProfile_Click(object sender, System.EventArgs e)
		{
			frmParent.btnNext_Click(sender, e);
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void btnExisting_Click(object sender, EventArgs e)
		{
			frmParent.eapConfig = frmParent.GetSelfExtractingEap();
			frmParent.LoadFrmSummary();
		}
	}
}
