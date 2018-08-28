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
    public partial class frmLocal : Form
    {
        readonly frmParent frmParent;

        public frmLocal(frmParent parentInstance)
        {
            frmParent = parentInstance;
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string dialogTitle = "";
            string dialogFilter = "";
            switch (frmParent.LblLocalFileType)
            {
                case "EAPCONFIG":
                    dialogTitle = "Select EAP Config file";
                    dialogFilter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*";
                    break;
                case "CERT":
                    dialogTitle = "Select client certificate";
                    dialogFilter = "Certificate files (*.PFX, *.P12)|*.pfx;*.p12|All files (*.*)|*.*";
                    break;
            }
            // opens dialog to select file
            string selectedFilePath = FileDialog.GetFileFromDialog(dialogTitle, dialogFilter);
            // prints out filepath
            txtFilepath.Text = selectedFilePath;

            
        }
        
        public uint ConnectWithFile()
        {
            // validates the selected config file
            if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "EAP")) return 0;

            // gets content of config file
            string eapString = File.ReadAllText(txtFilepath.Text);
            uint eapType = 0;

            try
            {
                // creates EapConfig object from Eap string
                EapConfig eapConfig = ConnectToEduroam.GetEapConfig(eapString);
                // creates profile from EapConfig object
                eapType = ConnectToEduroam.Setup(eapConfig);
                // makes the institution Id accessible from parent form
                frmParent.LblInstText = eapConfig.InstitutionInfo.InstId;
            }
            catch (ArgumentException argEx)
            {
                if (argEx.Message == "interfaceId")
                {
                    MessageBox.Show("Could not establish a connection through your computer's wireless network interface. \n" +
                                    "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.",
                        "Network interface error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return eapType;
        }
        
        public bool InstallCertFile()
        {
            if (!FileDialog.ValidateFileSelection(txtFilepath.Text, "CERT")) return false;

            try
            {
                var certificate = new X509Certificate2(txtFilepath.Text, txtCertPassword.Text);
                return true;
            }
            catch (CryptographicException ex)
            {
                if ((ex.HResult & 0xFFFF) == 0x56)
                {
                    MessageBox.Show("The password you entered is incorrect.", "Certificate install",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Could not install certificate.\nException: " + ex.Message, "Certificate install",
                                     MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }
        
        // checks wether certificate requires password or not
        private void txtFilepath_TextChanged(object sender, EventArgs e)
        {
            if (frmParent.LblLocalFileType == "EAPCONFIG") return;

            var passwordRequired = false;
            try
            {
                var certificate = new X509Certificate2(txtFilepath.Text, "");
            }
            catch (CryptographicException ex)
            {
                if ((ex.HResult & 0xFFFF) == 0x56)
                {
                    passwordRequired = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (passwordRequired)
            {
                lblCertPassword.Visible = true;
                txtCertPassword.Visible = true;
                cboShowPassword.Visible = true;
            }
            else
            {
                lblCertPassword.Visible = false;
                txtCertPassword.Visible = false;
                txtCertPassword.Text = "";
                cboShowPassword.Visible = false;
                cboShowPassword.Checked = false;
                txtCertPassword.UseSystemPasswordChar = true;
            }
        }

        // unmasks password characters on screen
        private void cboShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtCertPassword.UseSystemPasswordChar = !cboShowPassword.Checked;
        }
    }
}
