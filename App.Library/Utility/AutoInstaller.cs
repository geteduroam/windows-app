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


            if (!isInstalled)
            {
                
                var result = MessageBox.Show($"{Settings.Settings.ApplicationIdentifier} is niet geïnstalleerd, wilt u deze applicatie installeren voor geavanceerde functionaliteiten?", "- Verzin iets leuks -", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    SelfInstaller.DefaultInstance.EnsureIsInstalled();
                    return false;
                }
            }
            else if (isInstalled && canBeUpdated)
            {
                var result = MessageBox.Show($"De geinstallerde {Settings.Settings.ApplicationIdentifier} is is ouder dan deze versie, wilt u deze applicatie updaten?", "- Verzin iets leuks -", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    SelfInstaller.DefaultInstance.EnsureIsInstalled();
                    return false;
                }
            } else
            {
                var msgResult = MessageBox.Show($"U heeft {Settings.Settings.ApplicationIdentifier} al geinstalleerd maar draait hem vanuit een foutieve locatie. Wilt u de juiste versie openen?", "- Verzin iets leuks -", MessageBoxButton.YesNo);
                if (msgResult == MessageBoxResult.Yes)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static void RemoveRunningExecutable()
    {
        SelfInstaller.DefaultInstance.RemoveRunningExecutable();
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