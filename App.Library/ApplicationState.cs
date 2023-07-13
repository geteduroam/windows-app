using System.Collections.Generic;

using App.Library.ViewModels;

using EduRoam.Connect;

namespace App.Library
{
    public class ApplicationState : NotifyPropertyChanged
    {
        private IdentityProvider selectedIdentityProvider;

        private IdentityProviderProfile selectedProfile;

        public ApplicationState()
        {
            this.NavigationHistory = new Stack<BaseViewModel>();
        }

        public IdentityProvider SelectedIdentityProvider
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

        public IdentityProviderProfile SelectedProfile
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

        public void Reset()
        {
            SelectedIdentityProvider = null;
            SelectedProfile = null;
            NavigationHistory.Clear();
        }

        public Stack<BaseViewModel> NavigationHistory { get; }
    }
}