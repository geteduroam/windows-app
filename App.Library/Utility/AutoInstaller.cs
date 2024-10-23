using EduRoam.Connect.Install;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        if (SelfInstaller.ThisExePath != SelfInstaller.DefaultInstance.InstallExePath)
        {

            var isInstalled = SelfInstaller.DefaultInstance.IsInstalled;
            var canBeUpdated = false;
            if (isInstalled)
            {
                canBeUpdated = SelfInstaller.DefaultInstance.CanBeUpdated();
            }

            if (isInstalled && canBeUpdated)
            {
                var result = MessageBox.Show($"De geinstallerde {Settings.Settings.ApplicationIdentifier} is is ouder dan deze versie, wilt u deze applicatie updaten?", "Update beschikbaar!", MessageBoxButton.YesNo);
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