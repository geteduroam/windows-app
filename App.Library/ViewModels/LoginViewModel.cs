using System;
using System.Threading.Tasks;

using EduRoam.Connect.Eap;

namespace App.Library.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
        }

        protected override bool CanNavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }
    }
}