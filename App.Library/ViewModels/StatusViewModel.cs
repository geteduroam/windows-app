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
        public override bool ShowMenu => true;
        public override bool ShowHelp => true;

        public bool ShowProfileStatus => this.status.ActiveProfile;
        public string ProfileName => this.status.ProfileName ?? string.Empty;
        public string TimeLeft => this.status.TimeLeft ?? string.Empty;

        private void SelectOtherInstitution()
        {
            this.Owner.Restart();
        }

        //
        //        if (status.ActiveProfile)
        //        {
        //            LoadPageInstalledProfile();
        //        }
        //        else if (ExtractedEapConfig != null)
        //        {
        //            // loads summary form so user can confirm installation
        //            eapConfig = ExtractedEapConfig;
        //            ExtractFlag = true;
        //            LoadPageProfileOverview();
        //        }
        //        else
        //        {
        //            LoadPageMainMenu();
        //        }
    }
}
