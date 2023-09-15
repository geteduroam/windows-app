using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;
using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CredentialsConnection : IConnection
    {
        private readonly CredentialsConnector connector;

        public CredentialsConnection(CredentialsConnector connector)
        {
            this.connector = connector;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(bool force)
        {
            Console.WriteLine(SharedResources.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{SharedResources.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{SharedResources.Password}: ");
            var password = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(userName, password);

            var status = this.connector.ValidateCredentials();
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
