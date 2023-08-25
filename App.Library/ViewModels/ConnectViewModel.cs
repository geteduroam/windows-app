using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks.Connectors;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class ConnectViewModel : BaseConnectViewModel
    {
        public ConnectViewModel(MainViewModel owner, EapConfig eapConfig, DefaultConnector connector)
            : base(owner, eapConfig, new DefaultConnection(connector))
        {
            Task.Run(this.ConnectAsync);
        }

        public override bool ShowNavigateNext => false;

        protected override async Task ConfigureAndConnectAsync(IList<string> messages)
        {
            var connectionProperties = new ConnectionProperties();

            this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);
        }
    }
}