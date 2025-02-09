using App.Library.Install;

using System.Windows;

namespace App.Library.Utility;

public static class AutoInstaller
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns false is the running executable is the WRONG version, true if you are running the correct one</returns>
    public static bool CheckIfInstalled()
    {
        if (SelfInstaller.RunningExePath != SelfInstaller.DefaultInstance.UserInstallExePath)
        {
            var isInstalled = SelfInstaller.DefaultInstance.IsUserInstalled;
            var canBeUpdated = false;
            if (isInstalled)
            {
                canBeUpdated = SelfInstaller.DefaultInstance.IsRunningNewerThanUserInstalled();
            }

            if (isInstalled && canBeUpdated)
            {
                var result = MessageBox.Show(string.Format(EduRoam.Localization.Resources.UpdateCurrentFileNewer, Settings.Settings.ApplicationName), EduRoam.Localization.Resources.UpdateAvailable, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    SelfInstaller.DefaultInstance.EnsureIsInstalled();
                    return false;
                }
            }
        }
        return true;
    }

    public static void StartApplicationFromInstallLocation()
    {
        SelfInstaller.DefaultInstance.StartApplicationFromInstallLocation();
    }
}


public record AutoInstallerResultObject
{
    public bool IsInstalled { get; set; }
    public bool CanBeUpdated { get; set; } 
}