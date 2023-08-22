using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.Connections
{
    internal class CredentialsConnection : IConnection
    {
        private readonly CredentialsConnector connector;

        public CredentialsConnection(CredentialsConnector connector)
        {
            this.connector = connector;

        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(ConnectionProperties properties)
        {
            var userName = properties.UserName ?? throw new ArgumentNullException(nameof(properties));
            var password = properties.Password ?? throw new ArgumentNullException(nameof(properties));

            this.connector.Credentials = new ConnectorCredentials(userName, password);

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
