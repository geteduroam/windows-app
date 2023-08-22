using EduRoam.Connect.Eap;

using System;
using System.Threading.Tasks;

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
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        public bool ShowRules
        {
            get
            {
                return false;
            }
        }
    }
}