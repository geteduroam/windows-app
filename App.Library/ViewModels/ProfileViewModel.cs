using System;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public ProfileViewModel(MainViewModel mainViewModel, EapConfig eapConfig)
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