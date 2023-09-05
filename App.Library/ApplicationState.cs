using App.Library.ViewModels;

using EduRoam.Connect.Identity;

using System.Collections.Generic;

namespace App.Library
{
    public class ApplicationState : NotifyPropertyChanged
    {
        private IdentityProvider? selectedIdentityProvider;

        private IdentityProviderProfile? selectedProfile;

        private string searchText = string.Empty;

        public ApplicationState()
        {
            this.NavigationHistory = new Stack<BaseViewModel>();
        }

        public IdentityProvider? SelectedIdentityProvider
        {
            get
            {
                return this.selectedIdentityProvider;
            }
            set
            {
                this.selectedIdentityProvider = value;
                this.CallPropertyChanged();
            }
        }

        public IdentityProviderProfile? SelectedProfile
        {
            get
            {
                return this.selectedProfile;
            }
            set
            {
                this.selectedProfile = value;
                this.CallPropertyChanged();
            }
        }

        public string SearchText
        {
            get
            {
                return this.searchText;
            }
            set
            {
                this.searchText = value;
                this.CallPropertyChanged();
            }
        }

        public void Reset()
        {
            this.SelectedIdentityProvider = null;
            this.SelectedProfile = null;
            this.searchText = string.Empty;
            this.NavigationHistory.Clear();
        }

        public Stack<BaseViewModel> NavigationHistory { get; }
    }
}