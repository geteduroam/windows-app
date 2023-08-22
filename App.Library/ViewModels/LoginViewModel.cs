using App.Library.Connections;

using EduRoam.Connect.Eap;

using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string userName = string.Empty;
        private string password = string.Empty;
        private readonly EapConfig eapConfig;

        public LoginViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
            this.eapConfig = eapConfig;
        }

        protected override bool CanNavigateNextAsync()
        {
            return !string.IsNullOrWhiteSpace(this.userName) && !string.IsNullOrWhiteSpace(this.password);
        }

        protected override async Task NavigateNextAsync()
        {
            // Connect
            var connectionProperties = new ConnectionProperties()
            {
                UserName = this.userName,
                Password = this.password
            };

            await this.Owner.ConnectAsync(this.eapConfig, connectionProperties);
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