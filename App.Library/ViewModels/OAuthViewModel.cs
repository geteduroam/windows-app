using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;

using System;
using System.Threading.Tasks;
using System.Windows;

using SharedResources = EduRoam.Localization.Resources;

namespace App.Library.ViewModels
{
    public class OAuthViewModel : BaseViewModel
    {
        private readonly IdentityProviderProfile profile;

        public OAuthViewModel(MainViewModel owner)
            : base(owner)
        {
            this.profile = this.Owner.State.SelectedProfile ?? throw new ArgumentNullException(nameof(this.profile));

            Task.Run(
                async () =>
                {
                    var eapConfiguration = new EapConfigTask(new System.Threading.ManualResetEvent(false), new System.Threading.ManualResetEvent(false));

                    var eapConfig = await eapConfiguration.GetEapConfigAsync(this.profile.Id);

                    // Forces the window to be focussed again after returning from OAuth flow
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.MainWindow.Activate();
                        Application.Current.MainWindow.Focus();
                        Application.Current.MainWindow.Topmost = true;
                    });

                    if (eapConfig != null)
                    {
                        this.Owner.SetActiveContent(new CertificateViewModel(this.Owner, eapConfig));

                        return;
                    }
                });
        }

        public OAuthViewModel(MainViewModel owner, IdentityProviderProfile profile)
            : base(owner)
        {
            this.profile = profile;
        }

        public override string PageTitle => SharedResources.OAuthWaitingTitle;

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }
    }
}