using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class SelectInstitutionViewModel : BaseViewModel
    {
        private readonly IdentityProviderDownloader idpDownloader;

        private string searchText;

        public SelectInstitutionViewModel(MainViewModel owner, IdentityProviderDownloader idpDownloader)
            : base(owner)
        {
            this.idpDownloader = idpDownloader;
            this.searchText = string.Empty;
        }

        public string SearchText
        {
            get => this.searchText;
            set
            {
                this.searchText = value;
                this.CallPropertyChanged(string.Empty);
            }
        }

        public ObservableCollection<IdentityProvider> Institutions
        {
            get
            {
                if (string.IsNullOrEmpty(this.searchText))
                {
                    return new ObservableCollection<IdentityProvider>(this.idpDownloader.ClosestProviders);
                }

                return new ObservableCollection<IdentityProvider>(
                    this.idpDownloader.ClosestProviders.Where(
                        x => x.Name.ToLowerInvariant()
                              .StartsWith(this.searchText.ToLowerInvariant())));
            }
        }

        protected override bool CanNavigateNextAsync()
        {
            return this.Owner.State.SelectedIdentityProvider != null;
        }

        protected override Task NavigateNextAsync()
        {
            if (this.Owner.State.SelectedIdentityProvider.Profiles.Count
                == 1) // skip the profile select and go with the first one
            {
                var autoProfileId = this.Owner.State.SelectedIdentityProvider.Profiles.Single()
                                        .Id;
                if (!string.IsNullOrEmpty(autoProfileId))
                {
                    // if profile could not be handled then return to form
                    //if (!await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
                    //break;
                }
            }
            //LoadPageSelectProfile();

            this.Owner.SetActiveContent(new SelectProfileViewModel(this.Owner));
            return Task.CompletedTask;
            //this.CallPropertyChanged(string.Empty);
        }
    }
}