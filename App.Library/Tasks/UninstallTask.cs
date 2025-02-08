using App.Library.Install;
using EduRoam.Localization;

using System.Diagnostics;

namespace App.Library.Tasks
{
    public class UninstallTask
    {
        public static bool AppIsInstalled
        {
            get
            {
                return SelfInstaller.DefaultInstance.IsUserInstalled;
            }
        }
        public static bool Uninstall(bool showMessageBox)
        {
            // we cannot show a normal message box on success,
            // since we've dispatched a job to delete the running binary at this point
            // but we can spawn a PowerShell that will show the success message
            if (SelfInstaller.DefaultInstance.EnsureIsUserUninstalled())
            {
                if (showMessageBox)
                {
                    var messageBoxProcess = new ProcessStartInfo
                    {
                        FileName = "mshta",
                        Arguments = $"vbscript:Execute(\"msgbox \"\"{string.Format(Resources.UninstallNotification, Settings.Settings.ApplicationIdentifier)}\"\", 0, \"\"{string.Format(Resources.UninstallNotificationTitle, Settings.Settings.ApplicationIdentifier)}\"\":close\")",
                        WindowStyle = ProcessWindowStyle.Normal, // Shows a console in the taskbar, but it's hidden
                        CreateNoWindow = true,
                        WorkingDirectory = "C:\\"
                    };
                    Process.Start(messageBoxProcess);
                }
                return true;
            }
            return false;
        }
    }
}
