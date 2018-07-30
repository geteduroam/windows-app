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
    public partial class frm5 : Form
    {
        public frm5()
        {
            InitializeComponent();
        }

        private void frm5_Load(object sender, EventArgs e)
        {
            if (ConnectToEduroam.Connect())
            {
                lblStatus.Text = "Connected to eduroam.";
                pboStatus.Image = Properties.Resources.checkmark_16;
            }
        }
    }
}
