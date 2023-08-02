using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks.Connectors;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CertAndCertPassConnection : IConnection
    {
        private readonly CertAndCertPassConnector connector;
        private readonly FileInfo? certificateFile;

        public CertAndCertPassConnection(CertAndCertPassConnector connector, FileInfo? certificateFile)
        {
            this.connector = connector;
            this.certificateFile = certificateFile;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(bool force)
        {
            var status = TaskStatus.AsFailure();

            if (this.certificateFile == null)
            {
                var certificatePathOption = Options.GetCertificatePathOption();
                status.Errors.Add(string.Format(Resources.ErrorNoClientCertificateProvided, string.Join(", ", certificatePathOption.Aliases)));
                return status;
            }

            Console.Write($"{Resources.Passphrase}: ");
            var passphrase = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(passphrase);
            this.connector.CertificatePath = this.certificateFile;

            status = this.connector.ValidateCertificateAndCredentials();
            if (!status.Success)
            {
                return status;
            }

            status = await this.connector.ConfigureAsync(force);

            if (!status.Success)
            {
                return status;
            }

            return await this.connector.ConnectAsync();
        }
    }
}
