using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Device.Location;
using System.Xml;
using EduroamConfigure;
using Image = System.Drawing.Image;

namespace EduroamApp
{
    /// <summary>
    /// Main form.
    /// All other forms are loaded into a panel in this form.
    /// </summary>
    public partial class frmParent : Form
    {
        private enum FormId
        {
            Summary,
            SelectMethod,
            SelectInstitution,
            SelectProfile,
            Download,
            Local,
            Login,
            Connect,
            Redirect,
            LocalCert,
            SaveAndQuit
        }

        // private variables to be used in this form
        private FormId currentFormId;                                      // Id of currently selected form
        private readonly List<FormId> historyFormId = new List<FormId>();  // Keeps history of previously diplayed forms, in order to backtrack correctly
        private bool reload = true;                                        // Specifies wether a form is to be re-instantiated when loaded                                // Selected EAP configuration


        // makes forms globally accessible in parent form
        private frmSummary frmSummary;
        private frmSelectMethod frmSelectMethod;
        private frmSelectInstitution frmSelectInstitution;
        private frmSelectProfile frmSelectProfile;
        private frmDownload frmDownload;
        private frmLocal frmLocal;
        private frmConnect frmConnect;
        private frmLogin frmLogin;
        private frmRedirect frmRedirect;

        // public variables to be used across forms
        public GeoCoordinateWatcher GeoWatcher { get; set; }
        public EapConfig.AuthenticationMethod AuthMethod; // installed authmethod in EAP config
        public string InstId { get; set; }
        public string ProfileCondition { get; set; }
        public string LocalFileType { get; set; }
        public string RedirectUrl { get; set; }
        public bool ComesFromSelfExtract { get; set; }
        public bool SelfExtractFlag { get; set; }
        public bool SelectAlternative { get; set; }
        public bool EduroamAvailable { get; set; }
        public DateTime CertValidFrom { get; set; }
        public EapConfig eapConfig { get; set; }
        public int? idProviderId {get; set;}

        public Font font;
        public frmParent()
        {
            // starts GeoCoordinateWatcher when app starts
            GeoWatcher = new GeoCoordinateWatcher();
            GeoWatcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            // adds formClosed listener
            FormClosed += frmParent_FormClosed;
            eapConfig = null;
            InitializeComponent();
        }

        private void frmParent_Load(object sender, EventArgs e)
        {
            // sets eduroam logo
            webEduroamLogo.DocumentText = ImageFunctions.GenerateSvgLogoHtml(Properties.Resources.eduroam_logo, webEduroamLogo.Width, webEduroamLogo.Height);
            Icon = Properties.Resources.geteduroam;
            //this.Visible = true;
            //this.ShowInTaskbar = false;
            //this.Visible = true;
            //this.Hide();
            //this.Show();
            //this.WindowState = FormWindowState.Normal;
            //Activate();
            this.ShowInTaskbar = true;


            // checks if file came with self extract
            eapConfig = GetSelfExtractingEap();
            if (eapConfig != null)
            {
                // sets flags
                ComesFromSelfExtract = true;
                SelfExtractFlag = true;
                // reset web logo or else it won't load
                ResetLogo();
                // loads summary form so user can confirm installation
                LoadFrmSummary();
            }
            else
            {
                // goes to form for selecting install method
                LoadFrmSelectMethod();
            }
        }


        private void frmParent_LostFocus(object sender, EventArgs e)
        {

           // this.WindowState = FormWindowState.Minimized;
        }

        private void frmParent_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                //Hide();
            }
        }

        private void notifyIcon_MouseClick(object sender, EventArgs e)
        {
            Console.WriteLine("Notifycicon click");
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                //this.Show();
                //Focus();
                //this.TopMost = true;
                //this.TopMost = false;
                Activate();
            }
        }

        public void btnNext_Click(object sender, EventArgs e)
        {
            // creates new instances of forms when going forward
            reload = true;
            // adds current form to history for easy backtracking
            historyFormId.Add(currentFormId);

            switch (currentFormId)
            {
                // next form depends on EAP type of selected config
                case FormId.Summary:
                    if (SelectAlternative) // if user has config from self extract but wants to select another inst
                    {
                        ResetLogo();
                        LoadFrmSelectMethod();
                        break;
                    }
                    
                    string err;
                    (AuthMethod, err) = frmSummary.InstallEapConfig();
                    EduroamAvailable = true;

                    if (AuthMethod != null) // Profile was successfully installed
                    {
                        if (AuthMethod.NeedsClientCertificate()) {
                            LocalFileType = "CERT";
                            LoadFrmLocalCert();
                        }
                        else if (AuthMethod.NeedsLoginCredentials())
                        {
                            LoadFrmLogin();
                        }
                        else
                        { 
                            LoadFrmConnect();
                        }
                        break;
                    }

                    switch (err)
                    {
                        case "eduroam not available": // (no access point in range, or no WLAN service/device enabled)
                            EduroamAvailable = false;
                            LoadFrmSaveAndQuit();
                            break;
                        case "not supported":
                        case "exception occured":
                            break; // dialogbox should have already been produced
                        case "nothing installed":
                        default:
                            MessageBox.Show(
                                "Couldn't connect to eduroam. \n" +
                                "Your institution does not have a valid configuration.",
                                "Configuration not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                    break;

                // next form depends on radio button selection
                case FormId.SelectMethod:
                    SelfExtractFlag = false;
                    if (frmSelectMethod.newProfile) LoadFrmSelectInstitution();
                    else
                    {
                        
                        LoadFrmLocal();
                    }
                    break;

                case FormId.SelectInstitution:
                    var profiles = GetProfiles((int) frmSelectInstitution.idProviderId);
                    // if less than 2 profiles then, if a profile exists, autoselect it and go to Summary
                    if (profiles.Count < 2)
                    {
                        string autoProfileId = profiles.FirstOrDefault().Id;
                        HandleProfileSelect(autoProfileId);
                        break;
                    }
                    LoadFrmSelectProfile();
                    break;


                case FormId.SelectProfile:
                    string profileId = frmSelectProfile.ProfileId;
                    HandleProfileSelect(profileId);
                    break;

                // opens summary form if config is not null
                case FormId.Local:
                    eapConfig = frmLocal.LocalEapConfig();
                    if (eapConfig != null) LoadFrmSummary();
                    break;

                // lets user log in and opens connection form
                case FormId.Login:
                    if (frmLogin.connected)
                    {
                        Close();
                    }
                    frmLogin.ConnectWithLogin();
                    //LoadFrmConnect();
                    break;

                // closes application after successful connect
                case FormId.Connect:
                    Close();
                    break;

                // TODO: missing case Redirect. sanity is to throw on default

                // lets user select client cert and opens connection form
                case FormId.LocalCert:
                    if (frmLocal.InstallCertFile()) LoadFrmConnect();
                    break;

                // closes application after saving setup
                case FormId.SaveAndQuit:
                    Close();
                    break;
            }

            // removes current form from history if it gets added twice
            if (historyFormId.LastOrDefault() == currentFormId) historyFormId.RemoveAt(historyFormId.Count - 1);
        }

        /// <summary>
        /// Fetches a list of all eduroam institutions from https://cat.eduroam.org.
        /// </summary>
        private List<IdentityProviderProfile> GetProfiles(int providerId)
        {
            try
            {
                return IdentityProviderDownloader.GetIdentityProviderProfiles(providerId);
            }
            catch (EduroamAppUserError ex)
            {
                //lblError.Text = ex.UserFacingMessage;
            }
            return null;
        }

        // downloads eap config based on profileId
        // seperated into its own function as this can happen either through
        // user selecting a profile or a profile being autoselected
        private void HandleProfileSelect(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                MessageBox.Show("Please select a Profile.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                eapConfig = DownloadEapConfig(profileId);
            }
            catch (EduroamAppUserError ex) // TODO: register this in some higher level
            {
                MessageBox.Show(
                    ex.UserFacingMessage,
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                eapConfig = null;
            }
            if (eapConfig != null)
            {
                LoadFrmSummary();
            }
            else if (!string.IsNullOrEmpty(RedirectUrl))
            {
                LoadFrmRedirect();
            }
            return;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // reuses existing instances of forms when going backwards
            reload = false;
            // clears logo if going back from summary page
            if (currentFormId == FormId.Summary) ResetLogo();

            switch (historyFormId.Last())
            {
                case FormId.Summary:
                    if (SelfExtractFlag) // reloads the included config file if exists
                    {
                        eapConfig = GetSelfExtractingEap();
                    }
                    LoadFrmSummary();
                    break;
                case FormId.SelectMethod:
                    if (ComesFromSelfExtract) SelfExtractFlag = true; // enables back button if config file included in self extract
                    LoadFrmSelectMethod();
                    break;

                case FormId.SelectInstitution:
                    idProviderId = null;
                    LoadFrmSelectInstitution();
                    break;
                case FormId.SelectProfile:
                    LoadFrmSelectProfile();
                    break;
                case FormId.Download:
                    LoadFrmDownload();
                    break;
                case FormId.Local:
                    LoadFrmLocal();
                    break;
                case FormId.Login:
                    LoadFrmLogin();
                    break;
                case FormId.LocalCert:
                    LoadFrmLocalCert();
                    break;
                // TODO: missing cases? sanity is to throw on default
            }

            // removes current form from history
            historyFormId.RemoveAt(historyFormId.Count - 1);
        }

        /// <summary>
        /// Loads new form and shows it in content panel on parent form.
        /// </summary>
        /// <param name="nextForm">Instance of form to load.</param>
        private void LoadNewForm(Form nextForm)
        {
            nextForm.TopLevel = false;
            nextForm.AutoScroll = true;
            nextForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(nextForm);
            nextForm.Show();
        }

        /// <summary>
        /// Checks if an EAP-config file exists in the same folder as the executable
        /// </summary>
        /// <returns>EapConfig object if file exists, null if not.</returns>
        public EapConfig GetSelfExtractingEap()
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");

            if (files.Length <= 0) return null;
            try
            {
                string eapPath = files.First();
                string eapString = File.ReadAllText(eapPath);
                eapConfig = EapConfig.FromXmlData(eapString);
                return eapConfig;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        public EapConfig DownloadEapConfig(string profileId)
        {
            // checks if user has selected an institution and/or profile
            if (string.IsNullOrEmpty(profileId))
            {
                MessageBox.Show("Please select an institution and/or a profile.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // exits function if no institution/profile selected
            };
            IdentityProviderProfile profile = IdentityProviderDownloader.GetProfileFromId(profileId);
            string redirect = profile.redirect;
            // eap config file as string
            string eapString;

            // if OAuth
            if (profile.oauth)
            {
                // get eap config file from browser authenticate
                try
                {
                    OAuth oauth = new OAuth(profile.authorization_endpoint, profile.token_endpoint, profile.eapconfig_endpoint);
                    // generate authURI based on redirect
                    string authUri = oauth.GetAuthUri();
                    // get local listening uri prefix
                    string prefix = oauth.GetRedirectUri();
                    // browser authenticate
                    string responseUrl = GetResponseUrl(prefix, authUri);
                    // get eap-config string if available
                    eapString = oauth.GetEapConfigString(responseUrl);
                }
                catch (EduroamAppUserError ex)
                {
                    MessageBox.Show(ex.UserFacingMessage);
                    eapString = "";
                }
                // return focus to application
                Activate();
            }

            // if other redirect
            else if (!String.IsNullOrEmpty(redirect))
            {
                // makes redirect link accessible in parent form
                RedirectUrl = redirect;
                return null;
            }
            else
            {
                eapString = IdentityProviderDownloader.GetEapConfigString(profileId);
            }

            // if not empty, creates and returns EapConfig object from Eap string

            if (string.IsNullOrEmpty(eapString))
            {
                return null;
            }

            try
            {
                // if not empty, creates and returns EapConfig object from Eap string
                return EapConfig.FromXmlData(eapString);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(
                    "The selected institution or profile is not supported. " +
                    "Please select a different institution or profile.\n" +
                    "Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets a response URL after doing Browser authentication with Oauth authUri.
        /// </summary>
        /// <returns>response Url as string.</returns>
        public string GetResponseUrl(string redirectUri, string authUri)
        {
            string responseUrl; //= WebServer.NonblockingListener(redirectUri, authUri, parentLocation);
            using (var waitForm = new frmWaitDialog(redirectUri, authUri))
            {
                DialogResult result = waitForm.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return "";
                }
                responseUrl = waitForm.responseUrl;
            }
            return responseUrl;
        }

        public PictureBox PbxLogo => pbxLogo;
        public WebBrowser WebLogo => webLogo;
        public WebBrowser WebEduroamLogo => webEduroamLogo;

        public bool BtnNextEnabled
        {
            get => btnNext.Enabled;
            set
            {
                btnNext.Enabled = value;
                btnNext.ForeColor = System.Drawing.SystemColors.ControlLight;
                if (value)
                {
                    btnNext.BackColor = System.Drawing.SystemColors.Highlight;
                }
                else
                {
                    btnNext.BackColor = System.Drawing.SystemColors.GrayText;
                }
            }
        }

        public string BtnNextText
        {
            get => btnNext.Text;
            set => btnNext.Text = value;
        }

        public bool BtnBackEnabled
        {
            get => btnBack.Enabled;
            set => btnBack.Enabled = value;
        }

        public bool BtnBackVisible
        {
            get => btnBack.Visible;
            set => btnBack.Visible = value;
        }

        public bool BtnCancelEnabled
        {
            get => btnCancel.Enabled;
            set => btnCancel.Enabled = value;
        }


        /// <summary>
        /// Loads form that shows summary of selected EAP configuration.
        /// </summary>
        public void LoadFrmSummary()
        {
            if (reload) frmSummary = new frmSummary(this, eapConfig);
            currentFormId = FormId.Summary;
            // changes controls depending on where the summary form is called from
            if (SelfExtractFlag)
            {
                lblTitle.Text = "eduroam Setup";
                btnBack.Visible = false;
            }
            else
            {
                lblTitle.Text = "Profile Info";
            }
            if (!reload) BtnNextEnabled = true;
            btnNext.Text = eapConfig.AuthenticationMethods.First().EapType == EduroamConfigure.EapType.TLS ? "Connect" : "Next >";
            LoadNewForm(frmSummary);
        }

        /// <summary>
        /// Loads form that lets user choose how they want to get config file.
        /// </summary>
        public void LoadFrmSelectMethod()
        {
            frmSelectMethod = new frmSelectMethod(this);
            currentFormId = FormId.SelectMethod;
            lblTitle.Text = "Configure Eduroam to get started";
            //btnNext.Text = "Setup Eduroam";
            btnNext.Visible = false;
            // if config file exists in self extract but user wants to choose another institution
            //btnBack.Visible = ComesFromSelfExtract;
            btnBack.Visible = ComesFromSelfExtract;
            LoadNewForm(frmSelectMethod);
        }

        public void LoadFrmSelectInstitution()
        {
            currentFormId = FormId.SelectInstitution;
            frmSelectInstitution = new frmSelectInstitution(this);
            lblTitle.Text = "Select your institution";
            BtnNextEnabled = !reload;
            btnNext.Visible = true;
            btnNext.Text = "Next >";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmSelectInstitution);
        }

        public void LoadFrmSelectProfile()
        {
            currentFormId = FormId.SelectProfile;
            frmSelectProfile = new frmSelectProfile(this, (int) frmSelectInstitution.idProviderId);
            lblTitle.Text = "Select your profile";
            LoadNewForm(frmSelectProfile);
        }

        /// <summary>
        /// Loads form that lets user select institution and download config file.
        /// </summary>
        public void LoadFrmDownload()
        {
            if (reload) frmDownload = new frmDownload(this);
            currentFormId = FormId.Download;
            lblTitle.Text = "Select your institution";
            BtnNextEnabled = !reload;
            btnNext.Visible = true;
            btnNext.Text = "Next >";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmDownload);
        }

        /// <summary>
        /// Loads form that lets user select config file from computer.
        /// </summary>
        public void LoadFrmLocal()
        {
            LocalFileType = "EAPCONFIG";
            if (reload) frmLocal = new frmLocal(this);
            currentFormId = FormId.Local;
            lblTitle.Text = "Select EAP-config file";
            BtnNextEnabled = true;
            btnNext.Visible = true;
            btnNext.Text = "Next >";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmLocal);
        }

        /// <summary>
        /// Loads form that lets user log in with username+password.
        /// </summary>
        public void LoadFrmLogin()
        {
            frmLogin = new frmLogin(this);
            currentFormId = FormId.Login;
            lblTitle.Text = "Log in";
            BtnNextEnabled = false;
            btnNext.Text = "Connect";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmLogin);
        }

        /// <summary>
        /// Loads form that shows connection status.
        /// </summary>
        public void LoadFrmConnect()
        {
            frmConnect = new frmConnect(this);
            currentFormId = FormId.Connect;
            lblTitle.Text = "Connection status";
            BtnNextEnabled = false;
            btnBack.Enabled = false;
            btnBack.Visible = true;
            LoadNewForm(frmConnect);
        }

        /// <summary>
        /// Loads form that shows redirect link.
        /// </summary>
        public void LoadFrmRedirect()
        {
            frmRedirect = new frmRedirect(this);
            currentFormId = FormId.Redirect;
            lblTitle.Text = "You are being redirected";
            BtnNextEnabled = false;
            btnNext.Text = "Next >";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmRedirect);
        }

        /// <summary>
        /// Loads form that lets you select a local client certificate file.
        /// </summary>
        public void LoadFrmLocalCert()
        {
            if (reload) frmLocal = new frmLocal(this);
            currentFormId = FormId.LocalCert;
            lblTitle.Text = "Select client certificate file";
            BtnNextEnabled = true;
            btnNext.Text = "Connect";
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmLocal);
        }

        /// <summary>
        /// Loads form that lets user save configuration and quit.
        /// </summary>
        public void LoadFrmSaveAndQuit()
        {
            frmConnect = new frmConnect(this);
            currentFormId = FormId.SaveAndQuit;
            lblTitle.Text = "eduroam not available"; // TODO: not obvious from function name
            btnNext.Text = "Save";
            BtnNextEnabled = true;
            btnBack.Enabled = false;
            btnBack.Visible = true;
            LoadNewForm(frmConnect);
        }

        // adds lines to panels on parent form
        /*private void pnlNavTop_Paint(object sender, PaintEventArgs e)
        {
            Pen grayPen = new Pen(Color.LightGray);
            //int width = pnlNavTop.Width;

            Point point1 = new Point(0, 0);
            //Point point2 = new Point(width, 0);

            // Draw line to screen.
            e.Graphics.DrawLine(grayPen, point1, point2);
        }*/


        /// <summary>
        /// Empties both logo controls and makes them invisible.
        /// </summary>
        public void ResetLogo()
        {
            // reset pbxLogo
            pbxLogo.Image = null;
            pbxLogo.Visible = false;

            // reset webLogo
            webLogo.Navigate("about:blank");
            if (webLogo.Document != null)
            {
                webLogo.Document.Write(string.Empty);
            }
            webLogo.Visible = false;
        }

        // closes form
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        // FormClosed listene
        private void frmParent_FormClosed(object sender, FormClosedEventArgs e)
        {
            // deletes bad profile on application exit if connection was unsuccessful
            if (ProfileCondition == "BADPROFILE")
            {
                ConnectToEduroam.RemoveAllProfiles();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void pnlNavigation_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
