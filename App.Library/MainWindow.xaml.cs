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
    }
}
