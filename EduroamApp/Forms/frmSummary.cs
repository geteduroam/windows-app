using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EduroamApp
{
    /// <summary>
    /// Displays a summary of EAP config that user is connecting with.
    /// </summary>
    public partial class frmSummary : Form
    {
        // makes the parent form accessible from this class
        private readonly frmParent frmParent;
        private readonly EapConfig eapConfig;

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

            // displays institution name
            lblInstName.Text =  instName;
            // displays prompt to accept Terms of use if they exist
            if (string.IsNullOrEmpty(tou))
            {
                lblToU.Location = new Point(3, 19);
                lblToU.Text = "Press Next to continue.";
                chkAgree.Checked = true;
            }
            else
            {
                lblToU.Text = "Agree to the Terms of Use and press Next to continue.";
                chkAgree.Visible = true;
                chkAgree.Checked = false;
            }
            // displays website, email and phone number
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

            // checks if email address is valid
            if (!emailAddress.Contains("@"))
            {
                // disables link, but still displays it
                lblEmail.Enabled = false;
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

            // gets institution logo encoded to base64
            string logoBase64 = eapConfig.InstitutionInfo.Logo;
            string logoFormat = eapConfig.InstitutionInfo.LogoFormat;
            // adds logo to form if exists
            if (!string.IsNullOrEmpty(logoBase64))
            {
                int cWidth = frmParent.PbxLogo.Width;
                int cHeight = frmParent.PbxLogo.Height;

                if (logoFormat == "image/svg+xml")
                {
                    frmParent.WebLogo.Visible = true;
                    frmParent.WebLogo.DocumentText =
                        "<!DOCTYPE html>" +
                        "<html>" +
                        "<head>" +
                            "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                        "<style>" +
                            "img {" +
                                "position: absolute;" +
                                "top: 0;" +
                                "left: 0;" +
                                "display: block;" +
                                "max-width:" + cWidth + "px;" +
                                "max-height:" + cHeight + "px;" +
                                "width: auto;" +
                                "height: auto;" +
                            "}" +
                        "</style>" +
                        "</head>" +
                        "<body>" +
                        "<img src=\'data:image/svg+xml;base64," + logoBase64 + "\'>" +
                        "</body>" +
                        "</html>";
                }
                else
                {

                    Image logo = ConnectToEduroam.Base64ToImage(logoBase64, logoFormat);
                    decimal hScale = decimal.Divide(cWidth, logo.Width);
                    decimal vScale = decimal.Divide(cHeight, logo.Height);
                    decimal pScale = vScale < hScale ? vScale : hScale;
                    Bitmap resizedLogo = ResizeImage(logo, (int) (logo.Width * pScale), (int) (logo.Height * pScale));
                    
                    frmParent.PbxLogo.Image = resizedLogo;
                    int lPad = cWidth - frmParent.PbxLogo.Image.Width;
                    frmParent.PbxLogo.Padding = new Padding(lPad/2, 0, 0, 0);
                    frmParent.PbxLogo.Visible = true;
                }
            }
            

            frmParent.SelectAlternative = false;
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
        /// <returns>EAP type of installed EapConfig.</returns>
        public uint InstallEapConfig()
        {
            /*if (EduroamNetwork.GetEduroamPack() == null)
            {

            }*/
            try
            {
                uint eapType = ConnectToEduroam.Setup(eapConfig);
                frmParent.InstId = eapConfig.InstitutionInfo.InstId;
                if (EduroamNetwork.GetEduroamPack() == null)
                {
                    eapType = 600;
                    frmParent.EduroamAvailable = false;
                }
                else frmParent.EduroamAvailable = true;
                frmParent.ProfileCondition = "BADPROFILE";
                return eapType;
            }
            catch (ArgumentException argEx)
            {
                if (argEx.Message == "interfaceId")
                {
                    MessageBox.Show(
                        "Could not establish a connection through your computer's wireless network interface. \n" +
                        "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.\nException: " + argEx.Message,
                        "eduroam", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong.\n" + "Please try connecting with another institution.\n\n" 
                                + "Exception: " + ex.Message, "eduroam Setup failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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


        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
