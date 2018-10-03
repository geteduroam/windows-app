using System;
using System.Windows.Forms;

namespace EduroamApp
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
