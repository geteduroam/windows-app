using System.Collections.Generic;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class SelectProfileViewModel : BaseViewModel
    {
        public SelectProfileViewModel(MainViewModel mainViewModel)//, IdentityProviderDownloader idpDownloader)
            : base(mainViewModel)
        {
            //this.idpDownloader = idpDownloader;
            //this.searchText = string.Empty;
            //this.NextCommand = new DelegateCommand(this.Next, this.CanGoNext);
        }

        public List<IdentityProviderProfile> Profiles => this.MainViewModel.State.SelectedIdentityProvider.Profiles;

        public IdentityProviderProfile SelectedProfile { get; set; }
    }
}