using EduRoam.Connect.Install;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks
{
    public class UninstallTask
    {
        public void Uninstall()
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
                            Arguments = "vbscript:Execute(\"msgbox \"\"The application and its configuration have been uninstalled\"\", 0, \"\"Uninstall geteduroam\"\":close\")",
                            WindowStyle = ProcessWindowStyle.Normal, // Shows a console in the taskbar, but it's hidden
                            CreateNoWindow = true,
                            WorkingDirectory = "C:\\"
                        };
                        Process.Start(extinguishMe);
                    }
                    else
                    {
                        throw new NotSupportedException("Message when uninstall did not succeed");
                        //MessageBox.Show(
                        //"geteduroam is not yet uninstalled! The uninstallation was aborted.",
                        //caption: "Uninstall geteduroam",
                        //MessageBoxButton.OK,
                        //MessageBoxImage.Error);
                    }

                    throw new NotSupportedException("shutdown(success);");
                    //shutdown(success);


                },
                doDeleteSelf: true);
        }
    }
}
