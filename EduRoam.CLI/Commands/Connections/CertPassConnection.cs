using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks.Connectors;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CertPassConnection : IConnection
    {
        private readonly CertPassConnector connector;

        public CertPassConnection(CertPassConnector connector)
        {
            this.connector = connector;
        }

        public async Task<(bool connected, IList<string> messages)> ConfigureAndConnectAsync(bool force)
        {
            Console.Write($"{Resource.Passphrase}: ");
            var passphrase = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(passphrase);

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
