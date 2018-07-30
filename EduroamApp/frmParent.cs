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
        int currentFormId = 0;
        frm1 frm1;
        frm2 frm2;
        frm3 frm3;
        frm4 frm4;
        frm5 frm5;        
        GeoCoordinateWatcher watcher; // gets coordinates of computer

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
            this.pnlContent.Controls.Clear();
            this.pnlContent.Controls.Add(nextForm);
            nextForm.Show();
        }

        private void Back()
        {

        }
        private void Next()
        {

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
            switch (currentFormId)
            {                
                case 1:
                    frm1.InstallSelfExtract();
                    break;
                case 2:
                    frm2.GoToForm();
                    break;
                case 3:
                    break;
                case 4:
                    if (frm4.validateFileSelection() == true) frm4.ConnectWithFile();
                    break;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
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
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Checks if an EAP-config file exists in the same folder as the executable
        /// </summary>
        /// <returns></returns>
        public bool ExistSelfExtract()
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(exeLocation, "*.eap-config");
            if (files.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GeoCoordinateWatcher GetWatcher()
        {
            return watcher;
        }
        
        /// <summary>
        /// Loads form with self extracted config file install.
        /// </summary>
        public void LoadFrm1()
        {
            // creates new instance of form1 if there is none, passes parent form instance as parameter
            if (frm1 == null) frm1 = new frm1(this);
            currentFormId = 1;
            LoadNewForm(frm1);
            btnNext.Text = "Install";
            btnBack.Visible = false;
        }

        /// <summary>
        /// Loads form that lets user choose how they want to get config file.
        /// </summary>
        public void LoadFrm2()
        {
            if (frm2 == null) frm2 = new frm2(this);
            currentFormId = 2;
            LoadNewForm(frm2);
            btnNext.Text = "Next >";
            btnBack.Visible = true;
            btnBack.Enabled = false;
        }

        /// <summary>
        /// Loads form that lets user select institution and download config file.
        /// </summary>
        public void LoadFrm3()
        {
            if (frm3 == null) frm3 = new frm3(this);
            currentFormId = 3;
            LoadNewForm(frm3);
            btnNext.Text = "Connect";
            btnBack.Enabled = true;
        }

        /// <summary>
        /// Loads form that lets user select config file from computer.
        /// </summary>
        public void LoadFrm4()
        {
            if (frm4 == null) frm4 = new frm4();
            currentFormId = 4;
            LoadNewForm(frm4);
            btnNext.Text = "Connect";
            btnBack.Enabled = true;
        }

        /// <summary>
        /// Loads form that lets user log in with username and password.
        /// </summary>
        public void LoadFrm5()
        {
            if (frm5 == null) frm5 = new frm5();
            currentFormId = 5;
            LoadNewForm(frm5);
            btnNext.Text = "Connect";
        }
    }
}
