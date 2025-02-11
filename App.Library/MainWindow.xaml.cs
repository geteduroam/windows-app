using App.Library.ViewModels;

using Microsoft.Extensions.Logging;

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
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

        private readonly ILogger<MainWindow> logger;

        // P/Invoke declarations
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public MainWindow(ILogger<MainWindow> logger, MainViewModel mainViewModel) : base()
        {
            this.logger = logger;

            this.InitializeComponent();

            this.MainViewModel = mainViewModel;
            this.MainViewModel.CloseApp = this.Close;

            this.DataContext = this.MainViewModel;

            this.Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;

            // Disable the maximize button
            this.SourceInitialized += (s, e) =>
            {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX);
            };

            // TODO: Can we do a string.Format directly in the XAML?
            this.WaitingConnection.Text = string.Format(EduRoam.Localization.Resources.NoConnection, Settings.Settings.ApplicationName);
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.LogCritical(e.Exception, "Exception not handled by the app");
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.MainViewModel.Dispose();
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = false;
        }
    }
}
