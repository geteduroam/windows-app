using System;
using System.Threading.Tasks;

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

        protected override Task GoNextAsync()
        {
            throw new NotImplementedException();
        }
    }
}