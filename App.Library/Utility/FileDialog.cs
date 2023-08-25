using EduRoam.Localization;

using Microsoft.Win32;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace App.Library.Utility
{
    internal static class FileDialog
    {
        /// <summary>
		/// Lets user select a file through an OpenFileDialog.
		/// </summary>
		/// <param name="dialogTitle">Title of the OpenFileDialog.</param>
		/// <param name="filter">Filter for OpenFileDialog.</param>
		/// <returns>Path of selected file.</returns>
		internal static string? GetFileFromDialog(string dialogTitle, string filter)
        {
            var fileDialog = new OpenFileDialog
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
		internal static bool ValidateFile(string filePath, List<string> fileTypes)
        {
            // checks if filepath is empty
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show(Resources.FileDialogSelectFile,
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // checks if filepath is valid
            if (!File.Exists(filePath))
            {
                MessageBox.Show(Resources.WarningFileDoesNotExist,
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // checks if file extension is valid
            var extensionSupported = fileTypes.Any(fileType => string.Equals(fileType, Path.GetExtension(filePath), System.StringComparison.CurrentCultureIgnoreCase));
            if (extensionSupported)
            {
                return true;
            }

            MessageBox.Show(Resources.WarningUnsupportedFileType,
                "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}
