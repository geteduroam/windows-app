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

        public TaskStatus ValidateCredentials()
        {
            if (this.Credentials == null || string.IsNullOrWhiteSpace(this.Credentials.Password))
            {
                return TaskStatus.AsFailure(Resource.ErrorInvalidCredentials);
            }

            return TaskStatus.AsSuccess();
        }

        public override async Task<TaskStatus> ConfigureAsync(bool forceConfiguration = false)
        {
            var status = this.ValidateCredentials();

            if (!status.Success)
            {
                return status;
            }

            status = await base.ConfigureAsync(forceConfiguration);

            if (!status.Success)
            {
                return status;
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificatePassphrase(this.Credentials!.Password.ToString()!);

            if (eapConfigWithPassphrase != null)
            {
                var exception = InstallEapConfig(eapConfigWithPassphrase);

                if (exception != null)
                {
                    status.Success = false;
                    status.Errors.Add(exception.Message);
                }
            }

            return status;
        }

        public override async Task<TaskStatus> ConnectAsync()
        {
            var status = this.ValidateCredentials();

            if (!status.Success)
            {
                return status;
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificatePassphrase(this.Credentials!.Password.ToString()!);

            status.Success = await Task.Run(ConnectToEduroam.TryToConnect);

            if (status.Success)
            {
                status.Messages.Add(Resource.Connected);
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(eapConfigWithPassphrase))
                {
                    status.Errors.Add(Resource.ErrorConfiguredButUnableToConnect);
                }
                else
                {
                    // Hs2 is not enumerable
                    status.Errors.Add(Resource.ErrorConfiguredButProbablyOutOfCoverage);
                }
            }

            return status;
        }
    }
}
