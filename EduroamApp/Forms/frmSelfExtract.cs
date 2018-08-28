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
using System.Diagnostics;

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
            // checks again if eap config file exists in directory
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");
            if (files.Length <= 0)
            {
                // loads form to let user select another Eap config file
                frmParent.LoadFrmSelectMethod();
                return;
            }

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
            
            string instName = eapConfig.InstitutionInfo.DisplayName;
            string tou = eapConfig.InstitutionInfo.TermsOfUse;
            string webAddress = eapConfig.InstitutionInfo.WebAddress.ToLower();
            string emailAddress = eapConfig.InstitutionInfo.EmailAddress.ToLower();

            lblInstName.Text = "You will now be connecting to " + instName + ".";
            if (string.IsNullOrEmpty(tou))
            {
                lblToU.Text = "Press Install to continue.";
                chkAgree.Checked = true;
            }
            else
            {
                lblToU.Text = "Agree to the Terms of Use and press Install to continue.";
                chkAgree.Visible = true;
                chkAgree.Checked = false;
            }
            lblWeb.Text = webAddress;
            lblEmail.Text = emailAddress;
            lblPhone.Text = eapConfig.InstitutionInfo.Phone;

            // checks if link starts with an accepted prefix
            if (webAddress.StartsWith("http://") || webAddress.StartsWith("https://") ||
                webAddress.StartsWith("www."))
            {
                // sets linkdata
                var redirectLink = new LinkLabel.Link {LinkData = lblWeb.Text};
                lblWeb.Links.Add(redirectLink);
            }
            // disables link, but still displays it
            else
            {
                lblWeb.Enabled = false;
            }

            foreach (Control cntrl in tblContactInfo.Controls)
            {
                if (string.IsNullOrEmpty(cntrl.Text))
                {
                    cntrl.Text = "-";
                    cntrl.Enabled = false;
                }
            }

            lblAlternate.Text = "Not connecting to " + eapConfig.InstitutionInfo.DisplayName + "?";

            // gets institution logo encoded to base64
            string logoBase64 = eapConfig.InstitutionInfo.Logo;
            // adds logo to form if exists
            if (string.IsNullOrEmpty(logoBase64))
            {
                frmParent.PbxLogo = ConnectToEduroam.Base64ToImage(logoBase64);
            }
            
        }

        private void lblWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // opens website in default browser
            Process.Start(e.Link.LinkData as string);
        }

        private void lblEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // opens default email software
            Process.Start("mailto:" + e.Link.LinkData);
        }

        public uint InstallSelfExtract()
        {
            string errorMessage;
            try
            {
                uint eapType = ConnectToEduroam.Setup(eapConfig);
                frmParent.LblInstText = eapConfig.InstitutionInfo.InstId;
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

            return 0;
        }

        private void btnSelectInst_Click(object sender, EventArgs e)
        {
            frmParent.LoadFrmSelectMethod();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chkAgree_CheckedChanged(object sender, EventArgs e)
        {
            frmParent.BtnNextEnabled = chkAgree.Checked;
        }
    }
}
