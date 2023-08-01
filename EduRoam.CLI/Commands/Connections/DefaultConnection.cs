using EduRoam.Connect.Tasks.Connectors;

namespace EduRoam.CLI.Commands.Connections
{
    internal class DefaultConnection : IConnection
    {
        private readonly DefaultConnector connector;

        public DefaultConnection(DefaultConnector connector)
        {
            this.connector = connector;
        }

        public async Task<(bool connected, IList<string> messages)> ConfigureAndConnectAsync(bool force)
        {
            var (configured, messages) = await this.connector.ConfigureAsync(force);

            if (!configured)
            {
                return (configured, messages);
            }

            return await this.connector.ConnectAsync();
        }
    }
}
