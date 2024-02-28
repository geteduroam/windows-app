using App.Library;
using App.Settings;

using Microsoft.Extensions.DependencyInjection;

using System.Linq;
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
            Settings.ApplicationIdentifier = "getgovroam";

            if (e.Args.Any()
                && CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
            }

            this.serviceProvider = ServicesConfiguration.ConfigureServices();

            var mainWindow = this.serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}