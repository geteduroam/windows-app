using EduRoam.Connect.Eap;

using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string userName = string.Empty;
        private string password = string.Empty;

        public LoginViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
        }

        protected override bool CanNavigateNextAsync()
        {
            return !string.IsNullOrWhiteSpace(this.userName); // && !string.IsNullOrWhiteSpace(this.password);
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

        public string UserName
        {
            get
            {
                return this.userName;

            }
            set
            {
                this.userName = value;
                this.CallPropertyChanged();
            }
        }

        public string Password
        {
            get
            {
                return this.password;

            }
            set
            {
                this.password = value;
                this.CallPropertyChanged();
            }
        }
    }
}