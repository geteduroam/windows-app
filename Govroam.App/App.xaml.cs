using System.Linq;
using System.Windows;

using App.Library;

namespace Govroam.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Any()
                && CommandLineArgumentsHandler.PreGuiCommandLineArgs(e.Args))
            {
                this.Shutdown(1);
            }

            var mainWindow = new MainWindow();
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.Show();
        }
    }
}