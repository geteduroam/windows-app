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
        
        public frmSelectMethod(frmParent parentInstance)
        {
            // gets parent form instance
            frmParent = parentInstance;

            InitializeComponent();
        }

        /// <summary>
        /// Checks which radio button is selected and loads corresponding form.
        /// </summary>
        public int GoToForm()
        {
            return rdbDownload.Checked ? 3 : 4;
        }
    }
}
