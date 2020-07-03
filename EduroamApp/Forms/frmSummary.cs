using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using EduroamConfigure;
using System.Threading.Tasks;

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
            this.Hide();
            
            // gets institution information from EapConfig object
            
            string instName = eapConfig.InstitutionInfo.DisplayName;
            string tou = eapConfig.InstitutionInfo.TermsOfUse;
            string webAddress = eapConfig.InstitutionInfo.WebAddress.ToLower();
            string emailAddress = eapConfig.InstitutionInfo.EmailAddress.ToLower();
            string nextOrConnect = eapConfig.AuthenticationMethods.First().EapType == EapType.TLS ? "Connect" : "Next";
            string description = eapConfig.InstitutionInfo.Description;

            

            this.ActiveControl = lblDesc;

            
            // displays prompt to accept Terms of use if they exist
            if (string.IsNullOrEmpty(tou))
            {
                pnlTerms.Visible = false;
            }
            else
            {
                termsOfUse = tou;
                lnkToU.Visible = true;


            }
            // displays website, email, phone number
            lblWeb.Text = webAddress;
            lblEmail.Text = emailAddress;
            lblPhone.Text = eapConfig.InstitutionInfo.Phone;

            // display profile description
            lblDesc.Text = description;



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
                tblSelectInst.Visible = true;
                tblSelectInst.BringToFront();
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
            byte[] logoBytes = eapConfig.InstitutionInfo.LogoData;
            string logoMimeType = eapConfig.InstitutionInfo.LogoMimeType;
            // adds logo to form if exists
            if (logoBytes.Length > 0)
            {
                // deactivate eduroam logo if institute has its own logo
                frmParent.WebEduroamLogo.Visible = false;
                // gets size of container
                int cWidth = frmParent.WebLogo.Width;
                int cHeight = frmParent.WebLogo.Height;

                if (logoMimeType == "image/svg+xml")
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

            frmParent.BtnNextEnabled = true;
            
            this.Show();
            
            

            //lblWarnings.Text = "";
            //var warnings = ConnectToEduroam.LookForWarningsInEapConfig(eapConfig).ToList();
            //foreach ((bool, string) warning in warnings)
            /*{
                lblWarnings.Text += warning.Item1 + warning.Item2 + "\n";
            }*/

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
        /// A tuple containing:
        ///     - the auth method which succeded installing, or null
        ///     - a string describing why the installation failed
        /// </returns>
        public ValueTuple<EapConfig.AuthenticationMethod, string> InstallEapConfig()
        {
            try
            {
                EapConfig.AuthenticationMethod authMethod = null;
                string err = "nothing installed"; // TODO: enum?

                if (!ConnectToEduroam.EapConfigIsSupported(eapConfig))
                {
                    MessageBox.Show(
                        "The profile you have selected is not supported by this application.",
                        "No supported authentification method found.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return (authMethod, "not supported");
                }

                // Install EAP config as a profile
                foreach (var authMethodInstaller in ConnectToEduroam.InstallEapConfig(eapConfig))
                {
                    // check if we need to find a client certificate first
                    if (authMethodInstaller.NeedsClientCertificate())
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            "The selected profile prefers a separately provided client certificate. Do you want to browse your local files for one?",
                            "Client certificate needed", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dialogResult != DialogResult.Yes) continue; // try some other auth method

                        var results = FileDialog.AskUserForClientCertificateBundle();
                        if (results == null) continue; // user aborted

                        (string certPath, string certPass) = results.Value;
                        authMethodInstaller.AddClientCertificate(certPath, certPass); // todo, check success
                    }

                    // warn user if we need to install any CAs
                    if (authMethodInstaller.NeedsToInstallCAs())
                        MessageBox.Show(
                            "You will now be prompted to install a Certificate Authority. \n" +
                            "In order to connect to eduroam, you need to accept this by pressing \"Yes\" in the following dialog.",
                            "Accept Certificate Authority", MessageBoxButtons.OK);

                    // install CAs
                    while (!authMethodInstaller.InstallCertificates())
                    {
                        // Ask user if he wants to retry
                        DialogResult retryCa = MessageBox.Show(
                            "CA not installed. \n" +
                            "In order to connect to eduroam, you must press \"Yes\" when prompted to install the Certificate Authority.",
                            "Accept Certificate Authority", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        if (retryCa == DialogResult.Cancel)
                            break;
                    }
                    if (authMethodInstaller.NeedsToInstallCAs()) break; // if user refused to install the CAs, abort
                    
                    // Everything is now in order, install the profile!
                    if (authMethodInstaller.InstallProfile())
                    {
                        // installation was a success!
                        authMethod = authMethodInstaller.AuthMethod;
                        err = null;
                        break;
                    }
                }

                frmParent.InstId = eapConfig.InstitutionInfo.InstId; // TODO: what does this do? Move to frmParent?

                if (!EduroamNetwork.IsEduroamAvailable())
                {
                    err = "eduroam not available";
                }

                frmParent.ProfileCondition = frmParent.ProfileStatus.Incomplete;

                return (authMethod, err);
            }
            catch (ArgumentException argEx) // TODO, handle in ConnectToEduroam or EduroamNetwork
            {
                if (argEx.Message == "interfaceId")
                {
                    // was this due to Guid.Empty, in that case this should be solved
                    MessageBox.Show(
                        "Could not establish a connection through your computer's wireless network interface.\n" +
                        "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.\n" +
                        "\n" +
                        "Exception: " + argEx.Message,
                        "eduroam", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                throw;
            }
            catch (CryptographicException cryptEx) // TODO, handle in ConnectToEduroam, thrown by certificate store .add()
            {
                MessageBox.Show(
                    "One or more certificates are corrupt. Please select an another file, or try again later.\n" +
                    "\n" +
                    "Exception: " + cryptEx.Message, 
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (EduroamAppUserError ex)
            {
                MessageBox.Show(
                    ex.UserFacingMessage,
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex) // TODO, handle in ConnectToEduroam or EduroamNetwork
            {
                MessageBox.Show(
                    "Something went wrong.\n" +
                    "Please try connecting with another institution, or try again later.\n" +
                    "\n" +
                    "Exception: " + ex.Message,
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return (null, "exception occured");
        }



        private void btnSelectInst_Click(object sender, EventArgs e)
        {
            // sets variable to signal that user wants to select another method
            frmParent.SelectAlternative = true;
            // calls button listener in parent form
            frmParent.btnNext_Click(sender, e);
        }

        private void lblDesc_Click(object sender, EventArgs e)
        {

        }
    }
}
