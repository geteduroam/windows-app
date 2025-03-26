using App.Library;
using App.Library.Utility;
using App.Settings;

using Microsoft.Extensions.DependencyInjection;

using System.Windows;

using LanguageResources = EduRoam.Localization.Resources;

namespace Govroam.App
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
            Settings.OAuthClientId = "app.getgovroam.win";
            Settings.ApplicationName = "getgovroam";
            Settings.NetworkName = "govroam";
            Settings.UpdateBaseUrl = "https://getgovroam.nl";
            Settings.HelpUrl = "https://govroam.nl/support";
            Settings.DiscoveryUrl = "https://getgovroam.nl/v3/discovery.json";

            // architecture check
            ArchitectureHelper.CheckArchitectureCompatability();

            if (CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args) && !Settings.IsIncompatibleVersion)
            {
                this.Shutdown(1);
                return;
            }

            #region SelfInstaller AutoInstall
            if (!Settings.IsIncompatibleVersion)
            {
                var resultObject = AutoInstaller.CheckIfInstalled();
                if (!resultObject)
                {
                    AutoInstaller.StartApplicationFromInstallLocation();
                    this.Shutdown(1);
                    return;
                }
            }
            #endregion

            this.serviceProvider = ServicesConfiguration.ConfigureServices();

            var mainWindow = this.serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}