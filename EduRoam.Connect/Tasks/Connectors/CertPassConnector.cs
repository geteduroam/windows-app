using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;

namespace EduRoam.Connect.Tasks.Connectors
{
    public class CertPassConnector : Connector
    {
        public CertPassConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.CertPass;

        public ConnectorCredentials? Credentials { get; set; }

        public (bool, IList<string>) ValidateCredentials()
        {
            if (this.Credentials == null || this.Credentials.Password.Length == 0)
            {
                return (false, Resource.ErrorInvalidCredentials.AsListItem());
            }

            return (true, Array.Empty<string>());
        }

        public override async Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var (configured, messages) = this.ValidateCredentials();

            if (!configured)
            {
                return (configured, messages);
            }

            (configured, messages) = await base.ConfigureAsync(forceConfiguration);

            if (!configured)
            {
                return (configured, messages);
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificatePassphrase(this.Credentials!.Password.ToString()!);

            if (eapConfigWithPassphrase != null)
            {
                var exception = InstallEapConfig(eapConfigWithPassphrase);

                if (exception != null)
                {
                    configured = false;
                    messages = exception.Message.AsListItem();
                }
            }

            return (configured, messages);
        }

        public override async Task<(bool connected, IList<string> messages)> ConnectAsync()
        {
            var (connected, messages) = this.ValidateCredentials();

            if (!connected)
            {
                return (connected, messages);
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificatePassphrase(this.Credentials!.Password.ToString()!);

            connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(eapConfigWithPassphrase))
                {
                    message = Resource.ErrorConfiguredButUnableToConnect;
                }
                else
                {
                    // Hs2 is not enumerable
                    message = Resource.ErrorConfiguredButProbablyOutOfCoverage;
                }
            }

            return (connected, message.AsListItem());
        }
    }
}
