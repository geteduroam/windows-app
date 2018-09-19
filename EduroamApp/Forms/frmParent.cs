using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Device.Location;
using Image = System.Drawing.Image;

namespace EduroamApp
{
    /// <summary>
    /// Main form.
    /// All other forms are loaded into a panel in this form.
    /// </summary>
    public partial class frmParent : Form
    {
        // private variables to be used in this form
        private int currentFormId;                                 // Id of currently selected form
        private readonly List<int> formHistory = new List<int>();  // Keeps history of previously diplayed forms, in order to backtrack correctly
        private bool reload = true;                                // Specifies wether a form is to be re-instantiated when loaded
        private readonly GeoCoordinateWatcher watcher;             // Gets coordinates of computer
        private EapConfig eapConfig = new EapConfig();             // Selected EAP configuration
        private uint eapType;                                      // EAP type of selected EAP config
        
        // makes forms globally accessible in parent form
        private frmSummary frmSummary;
        private frmSelectMethod frmSelectMethod;
        private frmDownload frmDownload;
        private frmLocal frmLocal;
        private frmConnect frmConnect;
        private frmLogin frmLogin;
        private frmRedirect frmRedirect;

        // public variables to be used across forms
        public string InstId;
        public string ProfileCondition;
        public string LocalFileType;
        public string RedirectUrl;
        public bool ComesFromSelfExtract;
        public bool SelfExtractFlag;
        public bool SelectAlternative;
        public bool EduroamAvailable;

        public frmParent()
        {
            // starts GeoCoordinateWatcher when app starts
            watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            // adds formClosed listener
            FormClosed += frmParent_FormClosed;
            InitializeComponent();
        }
        
        private void frmParent_Load(object sender, EventArgs e)
        {
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

        public void btnNext_Click(object sender, EventArgs e)
        {
            // creates new instances of forms when going forward
            reload = true;
            // adds current form to history for easy backtracking
            formHistory.Add(currentFormId);
            
            switch (currentFormId)
            {
                // next form depends on EAP type of selected config 
                case 1:
                    if (SelectAlternative) // if user has config from self extract but wants to select another inst
                    {
                        ResetLogo();
                        LoadFrmSelectMethod();
                        break;
                    }
                    eapType = frmSummary.InstallEapConfig();
                    if (eapType == 13) LoadFrmConnect();
                    else if (eapType == 25 || eapType == 21) LoadFrmLogin();
                    else if (eapType == 500)
                    {
                        LocalFileType = "CERT";
                        LoadFrmLocalCert();
                    }
                    else if (eapType == 600) LoadFrmSaveAndQuit();
                    else if (eapType != 0) MessageBox.Show("Couldn't connect to eduroam. \nYour institution does not have a valid configuration.",
                        "Configuration not valid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;

                // next form depends on radio button selection
                case 2:
                    SelfExtractFlag = false;
                    if (frmSelectMethod.GoToForm() == 3) LoadFrmDownload();
                    else
                    {
                        LocalFileType = "EAPCONFIG";
                        LoadFrmLocal();
                    }
                    break;

                // next form depends on if downloaded config contains redirect url or not
                case 3:
                    eapConfig = frmDownload.DownloadEapConfig();
                    if (eapConfig != null)
                    {
                        LoadFrmSummary();
                    }
                    else if (!string.IsNullOrEmpty(RedirectUrl))
                    {
                        LoadFrmRedirect();
                    }
                    break;

                // opens summary form if config is not null
                case 4:
                    eapConfig = frmLocal.LocalEapConfig();
                    if (eapConfig != null) LoadFrmSummary();
                    break;

                // lets user log in and opens connection form
                case 5:
                    if (eapType != 21)
                    {
                        frmLogin.ConnectWithLogin();
                        LoadFrmConnect();
                    }
                    else MessageBox.Show("Support for TTLS configuration not ready yet.", "TTLS not ready", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;

                // lets user select client cert and opens connection form
                case 8:
                    if (frmLocal.InstallCertFile()) LoadFrmConnect();
                    break;
                case 9:
                    ProfileCondition = "GOODPROFILE";
                    MessageBox.Show("Configuration saved. The application will now close.", "eduroam", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    break;
            }
            
            // removes current form from history if it gets added twice
            if (formHistory.LastOrDefault() == currentFormId) formHistory.RemoveAt(formHistory.Count - 1);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // reuses existing instances of forms when going backwards
            reload = false;
            // clears logo if going back from summary page
            if (currentFormId == 1) ResetLogo();

            switch (formHistory.Last())
            {
                case 1:
                    if (SelfExtractFlag) // reloads the included config file if exists
                    {
                        eapConfig = GetSelfExtractingEap();
                    }
                    LoadFrmSummary();
                    break;
                case 2:
                    if (ComesFromSelfExtract) SelfExtractFlag = true; // enables back button if config file included in self extract
                    LoadFrmSelectMethod();
                    break;
                case 3:
                    LoadFrmDownload();
                    break;
                case 4:
                    LoadFrmLocal();
                    break;
                case 5:
                    LoadFrmLogin();
                    break;
                case 8:
                    LoadFrmLocalCert();
                    break;
            }
            
            // removes current form from history
            formHistory.RemoveAt(formHistory.Count - 1);
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
                eapConfig = ConnectToEduroam.GetEapConfig(eapString);
                return eapConfig;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public PictureBox PbxLogo => pbxLogo;
        public WebBrowser WebLogo => webLogo;
        
        public string BtnNextText
        {
            get => btnNext.Text;
            set => btnNext.Text = value;
        }

        public bool BtnNextEnabled
        {
            get => btnNext.Enabled;
            set => btnNext.Enabled = value;
        }

        public bool BtnBackEnabled
        {
            get => btnBack.Enabled;
            set => btnBack.Enabled = value;
        }

        public string BtnCancelText
        {
            get => btnCancel.Text;
            set => btnCancel.Text = value;
        }

        public GeoCoordinateWatcher GetWatcher()
        {
            return watcher;
        }

        /// <summary>
        /// Loads form that shows summary of selected EAP configuration.
        /// </summary>
        public void LoadFrmSummary()
        {
            frmSummary = new frmSummary(this, eapConfig);
            currentFormId = 1;
            // changes controls depending on where the summary form is called from
            if (SelfExtractFlag)
            {
                lblTitle.Text = "eduroam Setup";
                btnBack.Visible = false;
            }
            else
            {
                lblTitle.Text = "Summary";
            }
            btnNext.Text = "Next >";
            btnNext.Enabled = true;
            LoadNewForm(frmSummary);
        }

        /// <summary>
        /// Loads form that lets user choose how they want to get config file.
        /// </summary>
        public void LoadFrmSelectMethod()
        {
            frmSelectMethod = new frmSelectMethod(this);
            currentFormId = 2;
            lblTitle.Text = "Certificate installation";
            btnNext.Text = "Next >";
            btnNext.Enabled = true;
            // if config file exists in self extract but user wants to choose another institution
            btnBack.Visible = ComesFromSelfExtract;
            LoadNewForm(frmSelectMethod);
        }

        /// <summary>
        /// Loads form that lets user select institution and download config file.
        /// </summary>
        public void LoadFrmDownload()
        {
            if (reload) frmDownload = new frmDownload(this);
            currentFormId = 3;
            lblTitle.Text = "Select your institution";
            btnNext.Text = "Next >";
            btnNext.Enabled = !reload;
            btnBack.Enabled = true;
            btnBack.Visible = true;
            LoadNewForm(frmDownload);
        }

        /// <summary>
        /// Loads form that lets user select config file from computer.
        /// </summary>
        public void LoadFrmLocal()
        {
            if (reload) frmLocal = new frmLocal(this);
            currentFormId = 4;
            lblTitle.Text = "Select EAP-config file";
            btnNext.Text = "Next >";
            btnNext.Enabled = true;
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
            currentFormId = 5;
            lblTitle.Text = "Log in";
            btnNext.Text = "Connect";
            btnNext.Enabled = false;
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
            currentFormId = 6;
            lblTitle.Text = "Connection status";
            btnNext.Text = "Next >";
            btnNext.Enabled = false;
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
            currentFormId = 7;
            lblTitle.Text = "You are being redirected";
            btnNext.Enabled = false;
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
            currentFormId = 8;
            lblTitle.Text = "Select client certificate file";
            btnNext.Text = "Next >";
            btnNext.Enabled = true;
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
            currentFormId = 9;
            lblTitle.Text = "eduroam not available";
            btnNext.Text = "Save";
            btnNext.Enabled = true;
            btnBack.Enabled = false;
            btnBack.Visible = true;
            LoadNewForm(frmConnect);
        }

        // adds lines to panels on parent form
        private void pnlNavTop_Paint(object sender, PaintEventArgs e)
        {
            Pen grayPen = new Pen(Color.LightGray);
            int width = pnlNavTop.Width;

            Point point1 = new Point(0, 0);
            Point point2 = new Point(width, 0);

            // Draw line to screen.
            e.Graphics.DrawLine(grayPen, point1, point2);
        }
        private void pnlLogoRight_Paint(object sender, PaintEventArgs e)
        {
            Pen grayPen = new Pen(Color.LightGray);
            int width = pnlLogoRight.Width;
            int height = pnlLogoRight.Height;

            Point point1 = new Point(width-1, 0);
            Point point2 = new Point(width-1, height);

            // Draw line to screen.
            e.Graphics.DrawLine(grayPen, point1, point2);
        }

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

        // FormClosed listener
        private void frmParent_FormClosed(object sender, FormClosedEventArgs e)
        {
            // deletes bad profile on application exit if connection was unsuccessful
            if (ProfileCondition == "BADPROFILE")
            {
                ConnectToEduroam.RemoveProfile();
            }
        }
    }
}
