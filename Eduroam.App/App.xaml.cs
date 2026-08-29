using App.Library;
using App.Library.Utility;
using App.Settings;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Windows;

using LanguageResources = EduRoam.Localization.Resources;

namespace Eduroam.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            LanguageResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;
            Settings.OAuthClientId = "app.geteduroam.win";
            Settings.ApplicationName = "geteduroam";
            Settings.NetworkName = "eduroam";
            Settings.UpdateBaseUrl = "https://dl.eduroam.app";
            Settings.HelpUrl = "https://geteduroam.app/";
            Settings.BrowserDownloadUrl = "https://www.eduroam.app/";
            Settings.DiscoveryUrl = "https://discovery.eduroam.app/v3/discovery.json";

            if (await CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
                return;
            }

            #region SelfInstaller AutoInstall
            var resultObject = AutoInstaller.CheckIfInstalled();
            if (!resultObject)
            {
                AutoInstaller.StartApplicationFromInstallLocation();
                this.Shutdown(1);
                return;
            }
            #endregion

            var serviceProvider = ServicesConfiguration.ConfigureServices();
            var mainWindow = serviceProvider.GetService<MainWindow>();
            if (mainWindow == null)
            {
                throw new Exception("MainWindow service not found.");
            }
            mainWindow.Show();
            mainWindow.Activate();
        }
    }
}