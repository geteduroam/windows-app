using EduRoam.Connect.Install;
using EduRoam.Connect.Language;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks
{
    public class UninstallTask
    {
        public void Uninstall(Action<bool> shutdown)
        {
            SelfInstaller.DefaultInstance.ExitAndUninstallSelf(
                success =>
                {
                    // we cannot show a normal message box on success,
                    // since we've dispatched a job to delete the running binary at this point
                    // but we can spawn a PowerShell that will show the success message
                    if (success)
                    {
                        var extinguishMe = new ProcessStartInfo
                        {
                            FileName = "mshta",
                            Arguments = $"vbscript:Execute(\"msgbox \"\"{Resource.UninstallNotification}\"\", 0, \"\"{Resource.UninstallNotificationTitle}\"\":close\")",
                            WindowStyle = ProcessWindowStyle.Normal, // Shows a console in the taskbar, but it's hidden
                            CreateNoWindow = true,
                            WorkingDirectory = "C:\\"
                        };
                        Process.Start(extinguishMe);
                    }
                    else
                    {
                        var extinguishMe = new ProcessStartInfo
                        {
                            FileName = "mshta",
                            Arguments = $"vbscript:Execute(\"msgbox \"\"{Resource.UninstallNotification}\"\", 0, \"\"{Resource.UninstallNotificationTitle}\"\":close\")",
                            WindowStyle = ProcessWindowStyle.Normal, // Shows a console in the taskbar, but it's hidden
                            CreateNoWindow = true,
                            WorkingDirectory = "C:\\"
                        };
                        Process.Start(extinguishMe);
                    }

                    shutdown(success);
                },
                doDeleteSelf: true);
        }
    }
}
