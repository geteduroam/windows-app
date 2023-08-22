using EduRoam.Connect.Tasks.Connectors;

using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.Connections
{
    internal class DefaultConnection : IConnection
    {
        private readonly DefaultConnector connector;

        public DefaultConnection(DefaultConnector connector)
        {
            this.connector = connector;
        }

        public async Task<TaskStatus> ConfigureAndConnectAsync(ConnectionProperties properties)
        {
            var status = await this.connector.ConfigureAsync(false);

            if (!status.Success)
            {
                return status;
            }

            return await this.connector.ConnectAsync();
        }
    }
}
