using EduRoam.Connect.Eap;
using EduRoam.Localization;

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

        public TaskStatus ValidateCertificateAndCredentials()
        {
            var status = new TaskStatus(true);

            if (this.Credentials == null || string.IsNullOrWhiteSpace(this.Credentials.Password))
            {
                status.Success = false;
                status.Errors.Add(Resources.ErrorInvalidCredentials);
            }

            if ((this.CertificatePath == null || !this.CertificatePath.Exists))
            {
                status.Success = false;
                status.Errors.Add(Resources.ErrorInvalidCertificatePath);
            }

            return status;
        }

        public override async Task<TaskStatus> ConfigureAsync(bool forceConfiguration = false)
        {
            var status = this.ValidateCertificateAndCredentials();

            if (!status.Success)
            {
                return status;
            }

            status = await base.ConfigureAsync(forceConfiguration);

            if (!status.Success)
            {
                return status;
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificate(this.CertificatePath!.FullName, this.Credentials!.Password.ToString()!);

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
            var status = this.ValidateCertificateAndCredentials();

            if (!status.Success)
            {
                return status;
            }

            Debug.Assert(
                !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                "Cannot configure EAP config that still needs credentials"
            );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                status.Success = false;
                status.Errors.Add(Resources.ErrorWirelessUnavailable);

                return status;
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificate(this.CertificatePath!.FullName, this.Credentials!.Password.ToString()!);

            status.Success = await Task.Run(ConnectToEduroam.TryToConnect);

            if (status.Success)
            {
                status.Messages.Add(ApplicationResources.GetString("Connected"));
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(eapConfigWithPassphrase))
                {
                    status.Errors.Add(Resources.ErrorConfiguredButUnableToConnect);
                }
                else
                {
                    // Hs2 is not enumerable
                    status.Errors.Add(Resources.ErrorConfiguredButProbablyOutOfCoverage);
                }
            }

            return status;
        }
    }
}
