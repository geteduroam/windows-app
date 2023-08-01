using EduRoam.Connect.Eap;

namespace EduRoam.Connect.Tasks.Connectors
{
    public class DefaultConnector : Connector
    {
        public DefaultConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.Default;

        public override async Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var (configured, messages) = await base.ConfigureAsync(forceConfiguration);

            if (configured)
            {
                var exception = this.InstallEapConfig(this.eapConfig);
                if (exception != null)
                {
                    configured = false;
                    messages = exception.Message.AsListItem();
                }
            }

            return (configured, messages);
        }
    }
}
