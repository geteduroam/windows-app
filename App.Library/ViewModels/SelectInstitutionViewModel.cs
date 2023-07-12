using System.Collections.ObjectModel;
using System.Linq;

using App.Library.Command;
using App.Library.Language;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class SelectInstitutionViewModel : BaseViewModel
    {
        private readonly IdentityProviderDownloader idpDownloader;

        private string searchText;

        public SelectInstitutionViewModel(MainViewModel mainViewModel, IdentityProviderDownloader idpDownloader)
            : base(mainViewModel)
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
                              .Contains(this.searchText.ToLowerInvariant())));
            }
        }

        public IdentityProvider SelectedIdentityProvider { get; set; }

        protected override bool CanGoNext()
        {
            return this.SelectedIdentityProvider != null;
        }

        protected override void GoNext()
        {
            this.MainViewModel.State.SelectedIdentityProvider = this.SelectedIdentityProvider;

            if (this.MainViewModel.State.SelectedIdentityProvider.Profiles.Count == 1) // skip the profile select and go with the first one
            {
                var autoProfileId = this.MainViewModel.State.SelectedIdentityProvider.Profiles.Single().Id;
                if (!string.IsNullOrEmpty(autoProfileId))
                {
                    // if profile could not be handled then return to form
                    //if (!await HandleProfileSelect(autoProfileId)) LoadPageSelectInstitution(refresh: false);
                    //break;
                }
            }
            //LoadPageSelectProfile();

            this.MainViewModel.SetActiveContent(new SelectProfileViewModel(this.MainViewModel));
            //this.CallPropertyChanged(string.Empty);
        }
    }
}