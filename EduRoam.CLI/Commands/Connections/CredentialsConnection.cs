using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

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
            Console.WriteLine(Resources.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{Resources.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{Resources.Password}: ");
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
