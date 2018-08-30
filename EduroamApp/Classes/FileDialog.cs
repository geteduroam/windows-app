using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
    /// <summary>
    /// Contains functions for selecting files through an OpenFileDialog
    /// </summary>
    class FileDialog
    {
        /// <summary>
        /// Lets user select a file through an OpenFileDialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the OpenFileDialog.</param>
        /// <param name="filter">Filter for OpenFileDialog.</param>
        /// <returns>Path of selected file.</returns>
        public static string GetFileFromDialog(string dialogTitle, string filter)
        {
            string filePath = null;

            OpenFileDialog fileDialog = new OpenFileDialog
            {
                // sets the initial directory of the open file dialog
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                // sets filter for file types that appear in open file dialog
                Filter = filter,
                FilterIndex = 0,
                RestoreDirectory = true,
                Title = dialogTitle
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }

            return filePath;
        }

        /// <summary>
        /// Checks if a config file has been selected, and if the filepath and type is valid.
        /// </summary>
        /// <returns>True if valid file, false if not.</returns>
        public static bool ValidateFileSelection(string filePath, string fileType)
        {
            // checks if filepath is empty
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Please select a file.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // checks if filepath is valid
            if (!File.Exists(filePath))
            {
                MessageBox.Show("The specified file does not exist.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // checks if file extension is valid
            var extensionSupported = false;
            switch (fileType)
            {
                case "EAP":
                    extensionSupported = Path.GetExtension(filePath) == ".eap-config";
                    break;
                case "CERT":
                    extensionSupported = (Path.GetExtension(filePath) == ".pfx" || Path.GetExtension(filePath) == ".p12");
                    break;
            }
            if (!extensionSupported)
            {
                MessageBox.Show("The file type you chose is not supported.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
