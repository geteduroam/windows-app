using App.Library.Command;
using App.Library.Connections;
using App.Library.Utility;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class ConnectWithLocalCertificatePassphraseViewModel : BaseConnectViewModel
    {
        private FileInfo? clientCertificate;
        private string passphrase = string.Empty;

        public ConnectWithLocalCertificatePassphraseViewModel(MainViewModel owner, EapConfig eapConfig, CertAndCertPassConnector connector)
            : base(owner, eapConfig, new CertAndCertPassConnection(connector))
        {
            this.SelectLocalCertificateCommand = new DelegateCommand(this.SelectLocalCertificate);
        }

        public DelegateCommand SelectLocalCertificateCommand { get; protected set; }

        protected override bool CanNavigateNextAsync()
        {
            return (
                (this.clientCertificate != null && this.clientCertificate.Exists) && !string.IsNullOrWhiteSpace(this.passphrase)
                );
        }

        public bool ShowRules
        {
            get
            {
                return false;
            }
        }

        public FileInfo? ClientCertificate
        {
            get
            {
                return this.clientCertificate;

            }
            set
            {
                this.clientCertificate = value;
                this.CallPropertyChanged();
            }
        }

        public string CertificatiePath => this.ClientCertificate?.FullName ?? string.Empty;

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
                CertificatePath = this.ClientCertificate,
                Passphrase = this.Passphrase
            };

            this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);
        }

        public void SelectLocalCertificate()
        {
            string? filepath;
            do
            {
                filepath = FileDialog.GetFileFromDialog(
                    Resources.LoadCertificateFile,
                    "Certificate files (*.PFX, *.P12)|*.pfx;*.p12|All files (*.*)|*.*");

                if (filepath == null)
                {
                    return; // the user canelled
                }
            }
            while (!FileDialog.ValidateFile(filepath, new List<string> { ".PFX", "*.P12" }));

            if (filepath == null)
            {
                this.ClientCertificate = null;
            }
            else
            {
                this.ClientCertificate = new FileInfo(filepath);
            }

            this.CallPropertyChanged(nameof(this.CertificatiePath));
        }
    }
}