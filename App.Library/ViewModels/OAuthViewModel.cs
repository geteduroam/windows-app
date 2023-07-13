using System;
using System.Threading.Tasks;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class OAuthViewModel : BaseViewModel
    {
        public OAuthViewModel(MainViewModel owner, IdentityProviderProfile profile)
            : base(owner)
        {
        }

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        protected override bool CanNavigatePrevious()
        {
            return false;
        }

        protected override Task NavigatePreviousAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}