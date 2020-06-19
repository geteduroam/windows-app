using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;

namespace EduroamApp
{
    /// <summary>
    /// Displays a summary of the EAP config that the user is connecting with.
    /// </summary>
    public partial class frmSummary : Form
    {
        // makes the parent form accessible from this class
        private readonly frmParent frmParent;
        private readonly EapConfig eapConfig;
        private string termsOfUse;

        public frmSummary(frmParent parentInstance, EapConfig configInstance)
        {
            // gets the parent form instance
            frmParent = parentInstance;
            // gets the Eap config instance
            eapConfig = configInstance;

            InitializeComponent();
        }

        private void frmSummary_Load(object sender, EventArgs e)
        {
            // gets institution information from EapConfig object
            string instName = eapConfig.InstitutionInfo.DisplayName;
            string tou = eapConfig.InstitutionInfo.TermsOfUse;
            string webAddress = eapConfig.InstitutionInfo.WebAddress.ToLower();
            string emailAddress = eapConfig.InstitutionInfo.EmailAddress.ToLower();
            string nextOrConnect = eapConfig.AuthenticationMethods.First().EapType == EapType.TLS ? "Connect" : "Next";

            // displays institution name
            lblInstName.Text =  instName;
            // displays prompt to accept Terms of use if they exist
            if (string.IsNullOrEmpty(tou))
            {
                lblToU.Location = new Point(3, 19);
                lblToU.Text = "Press " + nextOrConnect + " to continue.";
                chkAgree.Checked = true;
            }
            else
            {
                lblToU.Text = "Agree to the Terms of Use and press " + nextOrConnect + " to continue.";
                chkAgree.Text = "I agree to the";
                lnkToU.Text = "Terms of Use";
                termsOfUse = tou;
                chkAgree.Visible = true;
                lnkToU.Visible = true;
                chkAgree.Checked = false;
            }
            // displays website, email and phone number
            lblWeb.Text = webAddress;
            lblEmail.Text = emailAddress;
            lblPhone.Text = eapConfig.InstitutionInfo.Phone;



            // checks if website url is valid
            bool isValidUrl = Uri.TryCreate(webAddress, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isValidUrl)
            {
                // sets linkdata
                var redirectLink = new LinkLabel.Link {LinkData = webAddress};
                lblWeb.Links.Add(redirectLink);
            }
            // disables link, but still displays it
            else
            {
                lblWeb.Enabled = false;
            }

            // checks if email address is valid
            if (emailAddress.Contains(" ") || !emailAddress.Contains("@"))
            {
                // disables link, but still displays it
                lblEmail.Enabled = false;

                if (emailAddress.Contains("******"))
                {
                    lblEmail.Text = "-";
                }
            }

            // checks if phone number has numbers, disables label if not
            if (!lblPhone.Text.Any(char.IsDigit)) lblPhone.Enabled = false;

            // replaces empty fields with a dash
            foreach (Control cntrl in tblContactInfo.Controls)
            {
                if (string.IsNullOrEmpty(cntrl.Text))
                {
                    cntrl.Text = "-";
                }
            }

            // displays option to choose another institution if using file from self extract
            if (frmParent.SelfExtractFlag)
            {
                lblAlternate.Visible = true;
                btnSelectInst.Visible = true;
                lblAlternate.Text = "Not affiliated with " + eapConfig.InstitutionInfo.DisplayName + "?";
            }
            else
            {
                lblAlternate.Visible = false;
                btnSelectInst.Visible = false;
            }

            // sets flag
            frmParent.SelectAlternative = false;

            // gets institution logo encoded to base64
            byte[] logoBytes = eapConfig.InstitutionInfo.Logo;
            string logoFormat = eapConfig.InstitutionInfo.LogoFormat;
            // adds logo to form if exists
            if (logoBytes.Length > 0)
            {
                // gets size of container
                int cWidth = frmParent.PbxLogo.Width;
                int cHeight = frmParent.PbxLogo.Height;

                if (logoFormat == "image/svg+xml")
                {
                    frmParent.WebLogo.Visible = true;
                    frmParent.WebLogo.DocumentText = ImageFunctions.GenerateSvgLogoHtml(logoBytes, cWidth, cHeight);
                }
                else // other filetypes (jpg, png etc.)
                {
                    try
                    {
                        // converts from base64 to image
                        Image logo = ImageFunctions.BytesToImage(logoBytes);
                        decimal hScale = decimal.Divide(cWidth, logo.Width);
                        decimal vScale = decimal.Divide(cHeight, logo.Height);
                        decimal pScale = vScale < hScale ? vScale : hScale;
                        // resizes image to fit container
                        Bitmap resizedLogo = ImageFunctions.ResizeImage(logo, (int)(logo.Width * pScale), (int)(logo.Height * pScale));
                        frmParent.PbxLogo.Image = resizedLogo;
                        // centers image in container
                        int lPad = cWidth - frmParent.PbxLogo.Image.Width;
                        frmParent.PbxLogo.Padding = new Padding(lPad / 2, 0, 0, 0);
                        frmParent.PbxLogo.Visible = true;
                    }
                    catch (System.FormatException)
                    {
                        // ignore
                    }
                }
            }

        }

        private void lnkToU_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // displays terms of use in dialog form
            frmTermsOfUse termsDialog = new frmTermsOfUse(termsOfUse);
            termsDialog.ShowDialog();
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

        /// <summary>
        /// Installs certificates from EapConfig and creates wireless profile.
        /// </summary>
        /// <returns>
        /// EAP type (EapType) of installed EapConfig as a uint,
        /// or 500 if user needs to install a client certificate,
        /// or 600 if there is no connectivity,
        /// or 0 if some other error happened
        /// </returns>
        public uint InstallEapConfig()
        {
            try
            {
                uint eapType = 0;
                // Install EAP config as a profile
                foreach (var authMethodInstaller in ConnectToEduroam.InstallEapConfig(eapConfig))
                {
                    // warn user if we need to install CAs
                    if (authMethodInstaller.NeedToInstallCAs())
                        MessageBox.Show(
                            "You will now be prompted to install a Certificate Authority. \n" +
                            "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
                            "Accept Certificate Authority", MessageBoxButtons.OK);
                    while (!authMethodInstaller.InstallCertificates())
                    {
                        // Ask user if he wan'ts to retry
                        DialogResult retryCa = MessageBox.Show(
                            "CA not installed. \n" +
                            "In order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
                            "Accept Certificate Authority", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        if (retryCa == DialogResult.Cancel)
                            break;
                    }
                    if (authMethodInstaller.NeedToInstallCAs()) // if user refused to install CA
                        break;
                    if (!authMethodInstaller.InstallProfile())
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            "The selected profile requires a separate client certificate. Do you want to browse your local files for one?",
                            "Client certificate required", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        eapType = 500; // TODO: ew
                        break;
                    }
                    else // success
                    {
                        eapType = (uint)authMethodInstaller.EapType;
                        break;
                    }
                }

                frmParent.InstId = eapConfig.InstitutionInfo.InstId;

                if (EduroamNetwork.GetEduroamPack() == null)
                {
                    eapType = 600; // TODO: ew
                    frmParent.EduroamAvailable = false;
                }
                else frmParent.EduroamAvailable = true;

                frmParent.ProfileCondition = "BADPROFILE";
                return eapType;
            }
            catch (ArgumentException argEx) // TODO, handle in ConnectToEuroam or EduroamNetwork
            {
                if (argEx.Message == "interfaceId")
                {
                    MessageBox.Show(
                        "Could not establish a connection through your computer's wireless network interface.\n" +
                        "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.\n" +
                        "\n" +
                        "Exception: " + argEx.Message,
                        "eduroam", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (CryptographicException cryptEx) // TODO, handle in ConnectToEuroam or EduroamNetwork
            {
                MessageBox.Show(
                    "One or more certificates are corrupt. Please select another file, or try again later.\n" +
                    "\n" +
                    "Exception: " + cryptEx.Message, 
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex) // TODO, handle in ConnectToEuroam or EduroamNetwork
            {
                MessageBox.Show(
                    "Something went wrong.\n" +
                    "Please try connecting with another institution, or try again later.\n" +
                    "\n" +
                    "Exception: " + ex.Message, 
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return 0;
        }

        // enables Next button if user agrees to Terms of Use
        private void chkAgree_CheckedChanged(object sender, EventArgs e)
        {
            frmParent.BtnNextEnabled = chkAgree.Checked;
        }

        private void btnSelectInst_Click(object sender, EventArgs e)
        {
            // sets variable to signal that user wants to select another method
            frmParent.SelectAlternative = true;
            // calls button listener in parent form
            frmParent.btnNext_Click(sender, e);
        }

    }
}
