using System.Collections.Generic;
using System.Threading.Tasks;

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

        protected override async Task GoNextAsync()
        {
            // if profile could not be handled then return to form
            var result = await MainViewModel.HandleProfileSelect(this.SelectedProfile);
            //if (!await HandleProfileSelect(profileId)) 
            if (!result)
            {
                //todo what todo? stay here?
                //LoadPageSelectProfile(refresh: false);
            }
            //return Task.CompletedTask;
        }
    }
}