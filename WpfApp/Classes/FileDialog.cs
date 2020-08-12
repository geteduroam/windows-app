using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using EduroamConfigure;
using Microsoft.Win32;

namespace WpfApp.Classes
{
	class FileDialog
	{

		/// <summary>
		/// Asks the user to supply a user certificate bundle along with a valid password.
		/// Returns null if user aborted.
		/// </summary>
		/// <returns>(filepath, passphrase) or null</returns>
		public static string AskUserForClientCertificateBundle()
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
			return filepath;
		}

		/// <summary>
		/// Asks the user to supply a .eap-config file.
		/// Returns null if user aborted.
		/// </summary>
		/// <returns>EapConfig object or null</returns>
		public static EapConfig AskUserForEapConfig()
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
				return EduroamConfigure.EapConfig.FromXmlData(profileId: filepath, eapConfigXml);
			}
			catch (System.Xml.XmlException xmlEx)
			{
				MessageBox.Show(
					"The selected EAP config file is corrupted. Please choose another file.\n" +
					"Exception: " + xmlEx.Message,
					"eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (ArgumentException argEx)
			{
				MessageBox.Show(
					"Could not read from file. Make sure to select a valid EAP config file.\n" +
					"Exception: " + argEx.Message,
					"eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
				Filter = filter,
				FilterIndex = 0,
				RestoreDirectory = true,
				Title = dialogTitle
			};

			if (fileDialog.ShowDialog() == true)
			{
				return fileDialog.FileName;
			}

			return null;
		}

		/// <summary>
		/// Checks if a config file has been selected,
		/// and if the filepath and type extention is valid.
		/// </summary>
		/// <returns>True if valid file, false if not.</returns>
		public static bool ValidateFileSelection(string filePath, List<string> fileTypes)
		{
			// checks if filepath is empty
			if (string.IsNullOrEmpty(filePath))
			{
				MessageBox.Show(
					"Please select a file.",
					"Warning", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// checks if filepath is valid
			if (!File.Exists(filePath))
			{
				MessageBox.Show("The specified file does not exist.",
					"Warning", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// checks if file extension is valid
			var extensionSupported = fileTypes.Any(fileType => fileType == Path.GetExtension(filePath));
			if (extensionSupported) return true;

			MessageBox.Show(
				"The file type you chose is not supported.",
				"Warning", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
	}
}
