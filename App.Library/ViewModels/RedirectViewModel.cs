using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

using SharedResources = EduRoam.Localization.Resources;
namespace App.Library.ViewModels
{
    public class RedirectViewModel : BaseViewModel
    {
        private readonly Uri redirectUri;

        public RedirectViewModel(MainViewModel owner, Uri redirectUri)
            : base(owner)
        {
            this.redirectUri = redirectUri;
        }

        public override string PageTitle => SharedResources.RedirectTitle;

        protected override bool CanNavigateNextAsync()
        {
            return true;
        }

        protected override Task NavigateNextAsync()
        {
            Process.Start(new ProcessStartInfo(this.redirectUri.ToString()));
            Application.Current.Shutdown(1);
            return Task.CompletedTask;
        }
    }
}