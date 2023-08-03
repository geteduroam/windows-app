using App.Library;

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
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LanguageResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

            if (e.Args.Any()
                && CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}