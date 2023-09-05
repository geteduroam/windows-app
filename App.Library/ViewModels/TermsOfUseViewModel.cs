using EduRoam.Connect.Eap;

using System;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace App.Library.ViewModels
{
    public class TermsOfUseViewModel : BaseViewModel
    {
        private readonly EapConfig eapConfig;

        public TermsOfUseViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
            this.eapConfig = eapConfig;
        }

        public override string PageTitle => SharedResources.TermsOfUseTitle;

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotSupportedException();
        }

        public string TermsOfUse
        {
            get => this.eapConfig.InstitutionInfo.TermsOfUse;
        }
    }
}
