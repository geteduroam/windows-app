using System;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(MainViewModel mainViewModel, EapConfig eapConfig)
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