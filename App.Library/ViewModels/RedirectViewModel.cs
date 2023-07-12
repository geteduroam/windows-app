using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        protected override Task GoNextAsync()
        {
            Process.Start(new ProcessStartInfo(this.redirectUri.ToString()));
            Application.Current.Shutdown(1);
            return Task.CompletedTask;
        }
    }
}