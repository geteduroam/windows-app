using App.Library.Command;

using EduRoam.Connect.Tasks;

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
            this.status = new StatusTask().GetStatus();
        }

        public override string PageTitle => this.ShowProfileStatus ? this.ProfileName : string.Empty;

        public DelegateCommand SelectOtherInstitutionCommand { get; private set; }

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

        public string TimeLeft => this.status.TimeLeft ?? "-";

        private void SelectOtherInstitution()
        {
            this.Owner.Restart();
        }
    }
}
