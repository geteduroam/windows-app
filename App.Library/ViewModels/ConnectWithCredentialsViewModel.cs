using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class ConnectWithCredentialsViewModel : BaseConnectViewModel
    {
        private string userName = string.Empty;
        private string password = string.Empty;


        public ConnectWithCredentialsViewModel(MainViewModel owner, EapConfig eapConfig, CredentialsConnector connector)
            : base(owner, eapConfig, new CredentialsConnection(connector))
        {
        }

        protected override bool CanNavigateNextAsync()
        {
            return (
                !this.eapConfig.NeedsLoginCredentials ||
                (!string.IsNullOrWhiteSpace(this.userName) && !string.IsNullOrWhiteSpace(this.password))
                );
        }

        protected override Task NavigateNextAsync()
        {
            return this.ConnectAsync();
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

        public bool PasswordRequired => this.eapConfig.NeedsLoginCredentials;

        protected override async Task ConfigureAndConnectAsync(IList<string> messages)
        {
            var connectionProperties = new ConnectionProperties()
            {
                UserName = this.userName,
                Password = this.password
            };

            this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);
        }
    }
}