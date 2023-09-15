using EduRoam.Connect.Tasks.Connectors;

using System;
using System.IO;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;
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
                status.Errors.Add(string.Format(SharedResources.ErrorNoClientCertificateProvided, string.Join(", ", certificatePathOption.Aliases)));
                return status;
            }

            Console.Write($"{SharedResources.Passphrase}: ");
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
