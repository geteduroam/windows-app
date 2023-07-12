using System;
using System.Diagnostics;
using System.Windows;

namespace App.Library.ViewModels
{
    public class RedirectViewModel : BaseViewModel
    {
        private readonly Uri redirectUri;

        public RedirectViewModel(MainViewModel mainViewModel, Uri redirectUri)
            : base(mainViewModel)
        {
            this.redirectUri = redirectUri;
        }

        protected override bool CanGoNext()
        {
            return true;
        }

        protected override void GoNext()
        {
            Process.Start(new ProcessStartInfo(this.redirectUri.ToString()));
            Application.Current.Shutdown(1);
        }
    }
}