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
            if (rdbDownload.Checked)
            {
                return 3;
            }

            return 4;
        }

        private void rdbDownload_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void frm2_Load(object sender, EventArgs e)
        {

        }
    }
}
