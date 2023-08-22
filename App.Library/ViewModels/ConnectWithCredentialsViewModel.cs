using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.ViewModels
{
    public class ConnectWithCredentialsViewModel : BaseViewModel
    {
        private string userName = string.Empty;
        private string password = string.Empty;

        private readonly EapConfig eapConfig;

        private TaskStatus? connectionStatus;
        private CredentialsConnection credentialsConnector;

        public ConnectWithCredentialsViewModel(MainViewModel owner, EapConfig eapConfig, CredentialsConnector credentialsConnector)
            : base(owner)
        {
            this.eapConfig = eapConfig;
            this.credentialsConnector = new CredentialsConnection(credentialsConnector);
        }

        protected override bool CanNavigateNextAsync()
        {
            return (
                !this.eapConfig.NeedsLoginCredentials ||
                (!string.IsNullOrWhiteSpace(this.userName) && !string.IsNullOrWhiteSpace(this.password))
                );
        }

        protected override async Task NavigateNextAsync()
        {
            // Connect
            throw new NotImplementedException();
            this.CallPropertyChanged();
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

        public bool UserNameRequired => this.eapConfig.NeedsLoginCredentials;

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

        public bool Connected => this.connectionStatus?.Success ?? false;

        public TaskStatus? ConnectionStatus => this.connectionStatus;

        public bool PasswordRequired => this.eapConfig.NeedsLoginCredentials;
    }
}