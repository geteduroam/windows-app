using App.Library;
using App.Library.Utility;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.Extensions.DependencyInjection;

using System.Linq;
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

            #region SelfInstaller AutoInstall
            var resultObject = AutoInstaller.CheckIfInstalled();
            if(!resultObject)
            {
                AutoInstaller.RemoveRunningExecutable();
                AutoInstaller.StartApplicationFromInstallLocation();
                this.Shutdown(1);
            } 
            #endregion


            this.serviceProvider = ServicesConfiguration.ConfigureServices();

            var mainWindow = this.serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}