using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class SelectInstitutionViewModel : BaseViewModel
    {
        private string searchText;

        public SelectInstitutionViewModel(MainViewModel owner)
            : base(owner)
        {
            this.searchText = string.Empty;
            this.Institutions = new NotifyTaskCompletion<ObservableCollection<IdentityProvider>>(this.GetInstitutionsAsync());
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

        public NotifyTaskCompletion<ObservableCollection<IdentityProvider>> Institutions
        {
            get; private set;
        }

        public async Task<ObservableCollection<IdentityProvider>> GetInstitutionsAsync()
        {
            var institutesTask = new GetInstitutesTask();

            var institutes = await institutesTask.GetAsync(this.searchText);
            return new ObservableCollection<IdentityProvider>(institutes);
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