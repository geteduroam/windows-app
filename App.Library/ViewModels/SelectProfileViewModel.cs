using System.Collections.Generic;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class SelectProfileViewModel : BaseViewModel
    {
        public SelectProfileViewModel(MainViewModel mainViewModel) //, IdentityProviderDownloader idpDownloader)
            : base(mainViewModel)
        {
            //this.idpDownloader = idpDownloader;
            //this.searchText = string.Empty;
            //this.NextCommand = new DelegateCommand(this.Next, this.CanGoNext);
        }

        public List<IdentityProviderProfile> Profiles => this.MainViewModel.State.SelectedIdentityProvider.Profiles;

        public IdentityProviderProfile SelectedProfile { get; set; }

        protected override bool CanGoNext()
        {
            return this.SelectedProfile != null;
        }

        protected override void GoNext()
        {
            string profileId = this.SelectedProfile.Id;
            // if profile could not be handled then return to form
            //if (!await HandleProfileSelect(profileId)) LoadPageSelectProfile(refresh: false);
        }
    }
}