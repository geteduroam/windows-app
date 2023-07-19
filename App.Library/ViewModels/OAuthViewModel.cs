using EduRoam.Connect.Identity;

using System;
using System.Threading.Tasks;

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
    }
}