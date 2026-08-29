using App.Library;
using App.Library.Utility;
using App.Settings;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Windows;

using LanguageResources = EduRoam.Localization.Resources;

namespace Govroam.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            LanguageResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;
            Settings.OAuthClientId = "app.getgovroam.win";
            Settings.ApplicationName = "getgovroam";
            Settings.NetworkName = "govroam";
            Settings.UpdateBaseUrl = "https://getgovroam.nl";
            Settings.HelpUrl = "https://govroam.nl/support";
            Settings.BrowserDownloadUrl = "https://www.govroam.app/";
            Settings.DiscoveryUrl = "https://getgovroam.nl/v3/discovery.json";

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