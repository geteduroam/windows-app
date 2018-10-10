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
            tou = termsOfUse.Trim();
            InitializeComponent();
        }

        private void frmTermsOfUse_Load(object sender, EventArgs e)
        {
            txtToU.Text = tou;
        }

        private void txtToU_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}
