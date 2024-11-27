using App.Library;
using App.Library.Utility;
using Microsoft.Extensions.DependencyInjection;
using App.Settings;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

using LanguageResources = EduRoam.Localization.Resources;

namespace Eduroam.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LanguageResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

            if (e.Args.Any()
                && CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
            }

            #region Architecture Check
            if (
                (RuntimeInformation.ProcessArchitecture is not Architecture.Arm64 or Architecture.Arm  && ArchitectureHelper.IsArm64()) ||
                (RuntimeInformation.ProcessArchitecture is Architecture.Arm64 or Architecture.Arm && !ArchitectureHelper.IsArm64())
            )
            {
                Settings.IsArchitectureIncompatible = true;
            } 
            #endregion

            #region SelfInstaller AutoInstall
            if (!Settings.IsArchitectureIncompatible)
            {
                var resultObject = AutoInstaller.CheckIfInstalled();
                if (!resultObject)
                {
                    AutoInstaller.StartApplicationFromInstallLocation();
                    this.Shutdown(1);
                }
            }
            #endregion


            this.serviceProvider = ServicesConfiguration.ConfigureServices();

            var mainWindow = this.serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}