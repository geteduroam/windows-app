using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CertPassConnection : IConnection
    {
        private readonly CertPassConnector connector;

        public CertPassConnection(CertPassConnector connector)
        {
            this.connector = connector;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(bool force)
        {
            Console.Write($"{Resources.Passphrase}: ");
            var passphrase = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(passphrase);

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
