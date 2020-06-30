using System.Drawing;
using System.Windows.Forms;

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
			//this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
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
	}
}
