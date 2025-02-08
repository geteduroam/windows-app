using System.Windows;

using App.Library.Install;

using EduRoam.Localization;

namespace App.Library.Tasks
{
    public class InstallTask
    {
        public static void Install(bool showMessageBox)
        {
            SelfInstaller.DefaultInstance.EnsureIsInstalled();

            if (showMessageBox)
            {
                MessageBox.Show(
                    string.Format(Resources.InstallSuccess, Settings.Settings.ApplicationIdentifier),
                    caption: string.Format(Resources.InstallTitle, Settings.Settings.ApplicationIdentifier));
            }
        }
    }
}
