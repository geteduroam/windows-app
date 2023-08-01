using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks.Connectors;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CredentialsConnection : IConnection
    {
        private readonly CredentialsConnector connector;

        public CredentialsConnection(CredentialsConnector connector)
        {
            this.connector = connector;
        }

        public async Task<(bool connected, IList<string> messages)> ConfigureAndConnectAsync(bool force)
        {
            Console.WriteLine(Resource.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{Resource.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{Resource.Password}: ");
            var password = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(userName, password);

            var (validCredentials, messages) = this.connector.ValidateCredentials();
            if (!validCredentials)
            {
                return (validCredentials, messages);
            }

            (var configured, messages) = await this.connector.ConfigureAsync(force);

            if (!configured)
            {
                return (configured, messages);
            }

            return await this.connector.ConnectAsync();
        }
    }
}
