using App.Library.ViewModels;

using System.ComponentModel;
using System.Windows;

namespace App.Library
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly MainViewModel MainViewModel;

        public MainWindow()
        {
            this.InitializeComponent();

            this.MainViewModel = new MainViewModel();
            this.DataContext = this.MainViewModel;
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.MainViewModel.Dispose();
        }

        #region Non-MVVM bindings

        // The following events act as intermediate bindings, because direct binding
        // from Button.ContextMenu or its child elements does not work
        // A possible solution is described at: https://social.msdn.microsoft.com/Forums/vstudio/en-US/a4149979-6fcf-4240-a172-66122225d7bc/wpf-mvvm-contextmenu-binding-isopen-to-view-model?forum=wpf

        private void OpenMenu(object sender, RoutedEventArgs e)
        {
            this.ctMenuSettings.IsOpen = true;
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void LoadEapFile(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.LoadEapFile();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.Refresh();
        }

        private void Reauthenticate(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.Reauthenticate();
        }

        private void RemoveProfile(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.RemoveProfile();
        }

        private void RemoveCertificates(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.RemoveCertificates();
        }

        private void Uninstall(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.Uninstall(_ => this.Close());
        }

        #endregion
    }
}
