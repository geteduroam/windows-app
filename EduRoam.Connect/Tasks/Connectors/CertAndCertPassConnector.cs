using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks.Connectors
{
    public class CertAndCertPassConnector : Connector
    {
        public CertAndCertPassConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.CertAndCertPass;

        public ConnectorCredentials? Credentials { get; set; }
        public FileInfo? CertificatePath { get; set; }

        public (bool, IList<string>) ValidateCertificateAndCredentials()
        {
            var isValid = true;
            var messages = new List<string>();

            if (this.Credentials == null || this.Credentials.Password.Length == 0)
            {
                isValid = false;
                messages.Add(Resource.ErrorInvalidCredentials);
            }

            if ((this.CertificatePath == null || !this.CertificatePath.Exists))
            {
                isValid = false;
                messages.Add(Resource.ErrorInvalidCertificatePath);
            }

            return (isValid, messages);
        }

        public override async Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var (configured, messages) = this.ValidateCertificateAndCredentials();

            if (!configured)
            {
                return (configured, messages);
            }

            (configured, messages) = await base.ConfigureAsync(forceConfiguration);

            if (!configured)
            {
                return (configured, messages);
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificate(this.CertificatePath!.FullName, this.Credentials!.Password.ToString()!);

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
            var (connected, messages) = this.ValidateCertificateAndCredentials();

            if (!connected)
            {
                return (connected, messages);
            }

            Debug.Assert(
                !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                "Cannot configure EAP config that still needs credentials"
            );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable.AsListItem());
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificate(this.CertificatePath!.FullName, this.Credentials!.Password.ToString()!);

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
