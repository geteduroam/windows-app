using System.Drawing;
using System.Windows.Forms;
using System;
using EduroamConfigure;

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
		public bool NewProfile { get; set; }
		public bool BtnNewProfileEnabled
		{
			get => btnNewProfile.Enabled;
			set
			{
				btnNewProfile.Enabled = value;
				btnNewProfile.ForeColor = System.Drawing.SystemColors.ControlLight;
				if (value)
				{
					btnNewProfile.BackColor = System.Drawing.SystemColors.Highlight;
				}
				else
				{
					btnNewProfile.BackColor = System.Drawing.SystemColors.GrayText;
				}
			}
		}

		public frmSelectMethod(frmParent parentInstance)
		{
			// gets parent form instance
			frmParent = parentInstance;
			NewProfile = false;


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


			if (!frmParent.Online)
			{
				BtnNewProfileEnabled = false;
				label1.Text = "Could not reach geteduroam services. Either the servers are down or you are offline. You can still use local files or already configured profiles." +
					"Otherwise, connect to the internet and restart this application. ";
			}
			else
			{
				label1.Text = "Connect to quickly gain access to eduroam.\n" +
				"If you have previously downloaded a eap - config file from your institution  you can use this instead";
			}
		}



		private void btnNewProfile_Click(object sender, System.EventArgs e)
		{
			NewProfile = true;
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
