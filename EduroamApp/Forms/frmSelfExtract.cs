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
        private readonly frmParent frmParent;
        private EapConfig eapConfig;

        public frmSelfExtract(frmParent parentInstance)
        {
            // gets the parent form instance
            frmParent = parentInstance;
            InitializeComponent();            
        }

        private void frmSelfExtract_Load(object sender, EventArgs e)
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");
            if (files.Length > 0)
            {
                string eapPath = files.First();
                string eapString = File.ReadAllText(eapPath);
                try
                {
                    eapConfig = ConnectToEduroam.GetEapConfig(eapString);
                }
                catch (Exception)
                {
                    // loads form with alternate setup methods 
                    frmParent.LoadFrmSelectMethod();
                    return;
                }

                lblInstName.Text = "Your institution is " + eapConfig.InstitutionInfo.DisplayName + ".";
                lblWeb.Text = eapConfig.InstitutionInfo.WebAddress;
                lblEmail.Text = eapConfig.InstitutionInfo.EmailAddress;
                lblPhone.Text = eapConfig.InstitutionInfo.Phone;

                foreach (Control cntrl in tblContactInfo.Controls)
                {
                    if (string.IsNullOrEmpty(cntrl.Text))
                    {
                        cntrl.Text = "-";
                    }
                }
            }
            else
            {

            }

        }

        public uint InstallSelfExtract()
        {
            string errorMessage;
            try
            {
                uint eapType = ConnectToEduroam.Setup(eapConfig);
                return eapType;
            }
            catch (Exception ex)
            {
                errorMessage = "Something went wrong.\n" +
                                "Please try connecting through an alterate method.\n\n" +
                                "Exception: " + ex.Message;
            }

            MessageBox.Show(errorMessage, "eduroam Setup failed",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            pnlAltPopup.BackColor = Color.Red;
            pnlAltPopup.BorderStyle = BorderStyle.FixedSingle;

            return 0;
        }

        private void btnAltSetup_Click(object sender, EventArgs e)
        {
            // loads form with alternate setup methods 
            frmParent.LoadFrmSelectMethod();
        }


    }
}
