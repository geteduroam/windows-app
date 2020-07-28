using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace EduroamApp
{
    /// <summary>
    /// Contains functions for selecting files through an OpenFileDialog
    /// </summary>
    class FileDialog
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>(filecontents, passphrase) or null</returns>
        public static ValueTuple<string, string>? AskUserForClientCertificateBundle()
        {
            string filepath;
            do
            {
                filepath = GetFileFromDialog(
                    "Select a Client Certificate bundle",
                    "Certificate files (*.PFX, *.P12)|*.pfx;*.p12|All files (*.*)|*.*");

                if (filepath == null) return null; // the user canelled
            }
            while (!ValidateFileSelection(filepath, new List<string> { ".pfx", ".p12" }));

            var passphrase = "";
            while (!TestCertificatePassphrase(filepath, passphrase))
            {
                if (passphrase == "")
                {
                    MessageBox.Show(
                        "The certificate bundle you chose is password protected.\r\n" +
                        "Please provide a password",
                        "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "The password you provided was incorrect!\r\n" +
                        "Please try again",
                        "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // show password dialog
                passphrase = PasswordInputDialog("Please input the Client Certificate password",
                    "Client Certificate Password");
                if (passphrase == null) return null; // user canceled
            }
            return (filepath, passphrase);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>EapConfig object or null</returns>
        public static EduroamConfigure.EapConfig AskUserForEapConfig()
        {
            string filepath;
            do
            {
                filepath = GetFileFromDialog(
                    "Select a EAP Config file",
                    "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*");

                if (filepath == null) return null; // the user canelled
            }
            while (!ValidateFileSelection(filepath, new List<string> { ".eap-config" }));

            // read, validate, parse and return
            try
            {
                // read content of file
                string eapConfigXml = File.ReadAllText(filepath);

                // create and return EapConfig object
                return EduroamConfigure.EapConfig.FromXmlData(uid: filepath, eapConfigXml);
            }
            catch (System.Xml.XmlException xmlEx)
            {
                MessageBox.Show(
                    "The selected EAP config file is corrupted. Please choose another file.\n" +
                    "Exception: " + xmlEx.Message, 
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(
                    "Could not read from file. Make sure to select a valid EAP config file.\n" +
                    "Exception: " + argEx.Message,
                    "eduroam - Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }


        /// <summary>
        /// Lets user select a file through an OpenFileDialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the OpenFileDialog.</param>
        /// <param name="filter">Filter for OpenFileDialog.</param>
        /// <returns>Path of selected file.</returns>
        private static string GetFileFromDialog(string dialogTitle, string filter)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                // sets the initial directory of the open file dialog
                //InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                // sets filter for file types that appear in open file dialog
                Filter = filter, FilterIndex = 0,
                RestoreDirectory = true,
                Title = dialogTitle
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                return fileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Checks if a config file has been selected, and if the filepath and type extention is valid.
        /// </summary>
        /// <returns>True if valid file, false if not.</returns>
        public static bool ValidateFileSelection(string filePath, List<string> fileTypes)
        {
            // checks if filepath is empty
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show(
                    "Please select a file.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // TODO, flow control
            }

            // checks if filepath is valid
            if (!File.Exists(filePath))
            {
                MessageBox.Show("The specified file does not exist.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // TODO, flow control
            }

            // checks if file extension is valid
            var extensionSupported = fileTypes.Any(fileType => fileType == Path.GetExtension(filePath));
            if (extensionSupported) return true;

            MessageBox.Show(
                "The file type you chose is not supported.",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false; // TODO: flow control
        }

        private static bool TestCertificatePassphrase(string certificateFilePath, string passphrase)
        {
            try
            {
                var testCertificate = new X509Certificate2(certificateFilePath, passphrase); // TODO: any persist flags needed?
            }
            catch (CryptographicException ex)
            {
                if ((ex.HResult & 0xFFFF) == 0x56) return false; // wrong passphrase
                throw;
            }
            /*
            // TODO: remove
            catch (Exception)
            {
                // ignored
            }
            */
            return true;
        }



        /// https://stackoverflow.com/a/5427121
        private static string PasswordInputDialog(string text, string caption)
        {
            // simon TODO: style this ;)
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label()
            {
                Left = 50,
                Top = 20,
                Text = text
            };
            TextBox textBox = new TextBox()
            {
                Left = 50,
                Top = 50,
                Width = 200,
                UseSystemPasswordChar = true,
            };
            Button confirmation = new Button() {
                Text = "Submit",
                Left = 150,
                Width = 100,
                Top = 70,
                DialogResult = DialogResult.OK
            };

            confirmation.Click += (sender, e) => prompt.Close(); ;
            prompt.AcceptButton = confirmation;

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
