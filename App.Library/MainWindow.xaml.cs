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
        // private NotifyIcon notifyIcon;

        public readonly MainViewModel MainViewModel;

        public MainWindow()
        {
            this.InitializeComponent();

            //            this.ShowInTaskbar = true;

            //#pragma warning disable CA1416 // Validate platform compatibility
            //            this.notifyIcon = new()
            //            {
            //                Icon = new Icon(@"geteduroam.ico"),
            //                Visible = true,
            //                Text = "geteduroam"
            //            };
            //#pragma warning restore CA1416 // Validate platform compatibility

            this.MainViewModel = new MainViewModel(this.Close);
            this.DataContext = this.MainViewModel;
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.MainViewModel.Dispose();
        }
    }
}
