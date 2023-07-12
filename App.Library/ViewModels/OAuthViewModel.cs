using System;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class OAuthViewModel : BaseViewModel
    {
        public OAuthViewModel(MainViewModel mainViewModel, IdentityProviderProfile profile)
            : base(mainViewModel)
        {
        }

        protected override bool CanGoNext()
        {
            throw new NotImplementedException();
        }

        protected override void GoNext()
        {
            throw new NotImplementedException();
        }
    }
}