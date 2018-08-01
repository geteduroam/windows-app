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
using System.Device.Location;

namespace EduroamApp
{
    public partial class frmParent : Form
    {
        enum FormSituation
        {
            SelfExtract,
            SelectMethod,
            DownloadConnect,
            LocalConnect,
            LoginConnect
        }
        int currentFormId;
        int selectedMethodId;
        bool reload = true;
        frmSelfExtract frmSelfExtract;
        frmSelectMethod frmSelectMethod;
        frmDownload frmDownload;
        frmLocal frmLocal;
        frmConnect frmConnect;
        frmLogin frmLogin;
        readonly GeoCoordinateWatcher watcher; // gets coordinates of computer

        public frmParent()
        {
            // starts GeoCoordinateWatcher when app starts
            watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));

            InitializeComponent();
        }

        private void LoadNewForm(Form nextForm)
        {
            nextForm.TopLevel = false;
            nextForm.AutoScroll = true;
            nextForm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(nextForm);
            nextForm.Show();
        }
        
        
        private void frmParent_Load(object sender, EventArgs e)
        {
            // checks if file came with self extract
            if (ExistSelfExtract())
            {
                // goes to form for installation through self extract config file
                LoadFrm1();
            }
            else
            {
                // goes to form for selecting install method
                LoadFrm2();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // creates new instances of forms when going forward
            reload = true;
            switch (currentFormId)
            {                
                case 1:
                    frmSelfExtract.InstallSelfExtract();
                    break;
                case 2:
                    frmSelectMethod.GoToForm();
                    break;
                case 3:
                    if (frmDownload.ConnectWithDownload()) LoadFrm5();
                    break;
                case 4:
                    if (frmLocal.ConnectWithFile()) LoadFrm5();
                    break;
                case 5:
                    LoadFrm6();
                    break;
                case 6:

                    break;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // reuses existing instances of forms when going backwards
            reload = false;
            switch (currentFormId)
            {
                case 1:                    
                    break;
                case 2:                    
                    break;
                case 3:
                    LoadFrm2();
                    break;
                case 4:
                    LoadFrm2();
                    break;
                case 5:
                    if (selectedMethodId == 3) LoadFrm3();
                    else LoadFrm4();
                    break;
                case 6:
                    LoadFrm5();
                    break;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Checks if an EAP-config file exists in the same folder as the executable
        /// </summary>
        /// <returns>True if file exists, false if not.</returns>
        public bool ExistSelfExtract()
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");
            return files.Length > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GeoCoordinateWatcher GetWatcher()
        {
            return watcher;
        }

        // make button properties accessible from other forms
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

        /// <summary>
        /// Loads form with self extracted config file install.
        /// </summary>
        public void LoadFrm1()
        {
            // creates new instance of form1 if there is none, passes parent form instance as parameter
            if (reload) frmSelfExtract = new frmSelfExtract(this);
            currentFormId = 1;
            lblTitle.Text = "eduroam installer";
            btnNext.Text = "Install";
            btnBack.Visible = false;
            LoadNewForm(frmSelfExtract);
        }

        /// <summary>
        /// Loads form that lets user choose how they want to get config file.
        /// </summary>
        public void LoadFrm2()
        {
            if (reload) frmSelectMethod = new frmSelectMethod(this);
            currentFormId = 2;
            lblTitle.Text = "Certificate installation";
            btnNext.Text = "Next >";
            btnNext.Enabled = true;
            btnBack.Visible = true;
            btnBack.Enabled = false;
            LoadNewForm(frmSelectMethod);
        }

        /// <summary>
        /// Loads form that lets user select institution and download config file.
        /// </summary>
        public void LoadFrm3()
        {
            /*if (reload)*/ frmDownload = new frmDownload(this);
            currentFormId = 3;
            selectedMethodId = 3;
            lblTitle.Text = "Select your institution";
            btnNext.Text = "Connect";
            btnNext.Enabled = false;
            btnBack.Enabled = true;
            LoadNewForm(frmDownload);
        }

        /// <summary>
        /// Loads form that lets user select config file from computer.
        /// </summary>
        public void LoadFrm4()
        {
            if (reload) frmLocal = new frmLocal();
            currentFormId = 4;
            selectedMethodId = 4;
            lblTitle.Text = "Select EAP-config file";
            btnNext.Text = "Connect";
            btnBack.Enabled = true;
            LoadNewForm(frmLocal);
        }

        /// <summary>
        /// Loads form that shows connection status.
        /// </summary>
        public void LoadFrm5()
        {
            if (reload) frmConnect = new frmConnect(this);
            currentFormId = 5;
            lblTitle.Text = "Connection status";
            btnNext.Text = "Next >";
            btnNext.Enabled = false;
            btnBack.Enabled = false;
            LoadNewForm(frmConnect);
        }

        /// <summary>
        /// Loads form that lets user log in with username+password.
        /// </summary>
        public void LoadFrm6()
        {
            if (reload) frmLogin = new frmLogin();
            currentFormId = 6;
            lblTitle.Text = "Log in";
            btnNext.Text = "Connect";
            LoadNewForm(frmLogin);
        }
    }
}
