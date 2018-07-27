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
    public partial class frm4 : Form
    {
        public frm4()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // opens dialog to select EAP Config file
            string eapConfigPath = GetFileFromDialog("Select EAP Config file");
            // prints out filepath
            txtFilepath.Text = eapConfigPath;

            //string eapConfigString = File.ReadAllText(eapConfigPath);
        }

        /// <summary>
        /// Lets user select a file through an OpenFileDialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the OpenFileDialog.</param>
        /// <returns>Path of selected file.</returns>
        public string GetFileFromDialog(string dialogTitle)
        {
            string filePath = null;

            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.InitialDirectory = @"C:\Users\lwerivel18\source\repos\EduroamApp\EduroamApp\ConfigFiles"; // sets the initial directory of the open file dialog
            fileDialog.Filter = "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*"; // sets filter for file types that appear in open file dialog
            fileDialog.FilterIndex = 0;
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = dialogTitle;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }

            return filePath;
        }

        /// <summary>
        /// Checks wether a file is chosen during an open file dialog.
        /// </summary>
        /// <param name="filePath">Filepath returned from open file dialog.</param>
        /// <returns>True if valid filepath, false if not.</returns>
        public bool validateFileSelection(string filePath)
        {
            if (filePath == null)
            {
                MessageBox.Show("No file selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (Path.GetExtension(filePath) != ".eap-config")
            {
                MessageBox.Show("File type not supported.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
