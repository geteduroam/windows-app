﻿using App.Library.ViewModels;

using Microsoft.Extensions.Logging;

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

        private readonly ILogger<MainWindow> logger;

        public MainWindow(ILogger<MainWindow> logger) : base() 
        {
            this.logger = logger; 

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

            this.Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.LogCritical(e.Exception, "Exception not handled by the app");
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.MainViewModel.Dispose();
        }


    }
}
