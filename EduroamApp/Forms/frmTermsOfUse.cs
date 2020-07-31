using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace EduroamApp
{
    public partial class frmTermsOfUse : Form
    {
        private readonly string tou;

        public frmTermsOfUse(string termsOfUse)
        {
            _ = termsOfUse ?? throw new ArgumentNullException(paramName: nameof(termsOfUse));

            tou = termsOfUse.Trim();
            InitializeComponent();
        }

        // shows terms of use in text box
        private void frmTermsOfUse_Load(object sender, EventArgs e)
        {
            txtToU.Text = tou;
        }

        // makes links in terms of use clickable
        private void txtToU_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}
