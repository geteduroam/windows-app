using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Net;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Device.Location;

namespace EduroamApp
{
    public partial class frmSelfExtract : Form
    {
        // makes the parent form accessible from this class
        frmParent frmParent;

        public frmSelfExtract(frmParent parentInstance)
        {
            // gets the parent form instance
            frmParent = parentInstance;
            InitializeComponent();            
        }

        private void btnAltSetup_Click(object sender, EventArgs e)
        {
            // loads form with alternate setup methods 
            frmParent.LoadFrmSelectMethod();
        }

        public void InstallSelfExtract()
        {
            bool installFlag = false;
            string errorMessage = "";
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

            if (files.Length > 0)
            {
                string configPath = files.First();
                string configString = File.ReadAllText(configPath);
                try
                {
                    // ConnectToEduroam(configString)
                    installFlag = true;
                }
                catch (Exception ex)
                {
                    errorMessage = "Something went wrong.\n" +
                                    "Please try connecting through alternate setup.\n\n" +
                                    "Exception: " + ex.Message;
                }
            }
            else
            {
                errorMessage = "Error: Cannot find configuration file.\n" +
                                "Please try connecting through alternate setup.";
            }

            if (installFlag == false)
            {
                MessageBox.Show(errorMessage, "eduroam Setup failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                pnlAltPopup.BackColor = Color.Red;
                pnlAltPopup.BorderStyle = BorderStyle.FixedSingle;
            }

        }
    }
}
