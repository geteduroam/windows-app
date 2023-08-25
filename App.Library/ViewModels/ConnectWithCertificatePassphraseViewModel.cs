using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class ConnectWithCertificatePassphraseViewModel : BaseConnectViewModel
    {
        private string passphrase = string.Empty;


        public ConnectWithCertificatePassphraseViewModel(MainViewModel owner, EapConfig eapConfig, CertPassConnector connector)
            : base(owner, eapConfig, new CertPassConnection(connector))
        {
        }

        protected override bool CanNavigateNextAsync()
        {
            return !string.IsNullOrWhiteSpace(this.passphrase);
        }

        public bool ShowRules
        {
            get
            {
                return false;
            }
        }

        public string Passphrase
        {
            get
            {
                return this.passphrase;

            }
            set
            {
                this.passphrase = value;
                this.CallPropertyChanged();
            }
        }

        protected override async Task ConfigureAndConnectAsync(IList<string> messages)
        {
            var connectionProperties = new ConnectionProperties()
            {
                Passphrase = this.Passphrase
            };

            this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);
        }
    }
}