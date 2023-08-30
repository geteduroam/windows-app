using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.Connections
{
    internal class CertAndCertPassConnection : IConnection
    {
        private readonly CertAndCertPassConnector connector;

        public CertAndCertPassConnection(CertAndCertPassConnector connector)
        {
            this.connector = connector;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(ConnectionProperties properties)
        {
            var certificateFile = properties.CertificatePath ?? throw new ArgumentNullException(nameof(properties));
            var passphrase = properties.Password ?? throw new ArgumentNullException(nameof(properties));

            this.connector.Credentials = new ConnectorCredentials(passphrase);
            this.connector.CertificatePath = certificateFile;

            var status = this.connector.ValidateCertificateAndCredentials();
            if (!status.Success)
            {
                return status;
            }

            status = await this.connector.ConfigureAsync(false);

            if (!status.Success)
            {
                return status;
            }

            return await this.connector.ConnectAsync();
        }
    }
}
