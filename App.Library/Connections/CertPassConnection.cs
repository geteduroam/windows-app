using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.Connections
{
    internal class CertPassConnection : IConnection
    {
        private readonly CertPassConnector connector;

        public CertPassConnection(CertPassConnector connector)
        {
            this.connector = connector;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(ConnectionProperties properties)
        {
            var passphrase = properties.Passphrase ?? throw new ArgumentNullException(nameof(properties));

            this.connector.Credentials = new ConnectorCredentials(passphrase);

            var status = this.connector.ValidateCredentials();
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
