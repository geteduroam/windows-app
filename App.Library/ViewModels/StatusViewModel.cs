using App.Library.Command;

using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class StatusViewModel : BaseViewModel
    {
        private readonly Status status;

        public StatusViewModel(MainViewModel owner) : base(owner)
        {
            this.SelectOtherInstitutionCommand = new DelegateCommand(this.SelectOtherInstitution, () => true);
            this.RenewAccountCommand = new DelegateCommand(this.Reauthenticate, () => true);
            this.status = new StatusTask().GetStatus();
        }

        public override string PageTitle => this.ShowProfileStatus ? this.ProfileName : string.Empty;

        public DelegateCommand SelectOtherInstitutionCommand { get; private set; }

        public DelegateCommand RenewAccountCommand { get; private set; }

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        public override bool ShowNavigatePrevious => false;

        public override bool ShowNavigateNext => false;

        public override bool ShowLogo => true;

        public bool ShowProfileStatus => this.status.ActiveProfile;

        public string ProfileName => this.status.ProfileName ?? string.Empty;

        public bool ShowTimeLeft => !string.IsNullOrWhiteSpace(this.status.TimeLeft);

        public bool ShowRenewButton { 
            get {
                try
                {
                    var diffDate = (this.status.ExpirationDate - DateTime.Now);
                    if (!diffDate.HasValue || !this.ShowProfileStatus) return false;

                    return ((this.status.ExpirationDate - DateTime.Now).Value.Days <= Settings.Settings.DaysLeftForNotification);
                } catch(Exception ex)
                {
                    return false;
                }
            } 
        }

        public bool ShowRepairButton => this.status.ActiveProfile && !this.ShowRenewButton;

        public string TimeLeft => this.status.TimeLeft ?? "-";

        public string ConnectToResource => string.Format(Resources.ButtonAppConnect, Settings.Settings.NetworkName);

        private void SelectOtherInstitution()
        {
            this.Owner.SelectInstitution();
        }   
        
        private async void Reauthenticate()
        {
            // TODO: This probably doesn't try the refresh token
            await IdentityProviderDownloader.Instance.LoadProviders();
            this.Owner.Reauthenticate();
        }
    }
}
