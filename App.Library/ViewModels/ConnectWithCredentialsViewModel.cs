using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
#pragma warning disable CA1822 // Members are bound by a template and therefore cannot be static
    internal class ConnectWithCredentialsViewModel : BaseConnectViewModel
    {
        private string userName = string.Empty;
        private string password = string.Empty;

        public ConnectWithCredentialsViewModel(MainViewModel owner, EapConfig eapConfig, CredentialsConnector connector)
            : base(owner, eapConfig, new CredentialsConnection(connector))
        {
            var (realm, hint) = this.eapConfig.GetClientInnerIdentityRestrictions();

            Debug.WriteLine(hint);

            if (hint)
            {
                this.Realm = string.IsNullOrWhiteSpace(realm) ? Resources.UserNameWatermark : $"{Resources.ExampleRealm}@{realm}";
            } 
            else
            {
                this.Realm = "";
            }
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

        public string Realm
        {
            get;
        }

        public bool PasswordRequired => this.eapConfig.NeedsLoginCredentials;

        protected override async Task ConfigureAndConnectAsync(IList<string> messages)
        {
            var connectionProperties = new ConnectionProperties()
            {
                UserName = this.userName.EndsWith(this.Realm) ? this.userName : this.userName + this.Realm,
                Password = this.password
            };

            this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);
        }
    }
#pragma warning restore CA1822 // Mark members as static
}