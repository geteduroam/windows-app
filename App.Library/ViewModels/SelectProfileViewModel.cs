using EduRoam.Connect.Identity;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class SelectProfileViewModel : BaseViewModel
    {
        public SelectProfileViewModel(MainViewModel owner) //, IdentityProviderDownloader idpDownloader)
            : base(owner)
        {
            //this.idpDownloader = idpDownloader;
            //this.searchText = string.Empty;
            //this.NextCommand = new DelegateCommand(this.Next, this.CanGoNext);
        }

        public List<IdentityProviderProfile> Profiles => this.Owner.State.SelectedIdentityProvider?.Profiles ?? new List<IdentityProviderProfile>();

        protected override bool CanNavigateNextAsync()
        {
            return this.Owner.State.SelectedProfile != null;
        }

        protected override async Task NavigateNextAsync()
        {
            // if profile could not be handled then return to form
            var result = await this.Owner.HandleProfileSelect(this.Owner.State.SelectedProfile!);
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