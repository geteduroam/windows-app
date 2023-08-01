using EduRoam.Connect.Eap;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Language;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks.Connectors
{
    public partial class CredentialsConnector : Connector
    {

        public CredentialsConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.Credentials;

        public ConnectorCredentials? Credentials { get; set; }

        public (bool, IList<string>) ValidateCredentials()
        {
            if (this.Credentials == null || string.IsNullOrWhiteSpace(this.Credentials.UserName) || this.Credentials.Password.Length == 0)
            {
                return (false, Resource.ErrorInvalidCredentials.AsListItem());
            }

            var (realm, hint) = this.eapConfig.GetClientInnerIdentityRestrictions();

            var brokenRules = IdentityProviderParser.GetRulesBrokenOnUsername(this.Credentials.UserName, realm, hint);

            if (brokenRules.Any())
            {
                return (false, brokenRules.ToList());
            }

            return (true, Array.Empty<string>());
        }

        public override async Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var (configured, messages) = this.ValidateCredentials();

            if (!configured)
            {
                return (configured, messages);
            }

            (configured, messages) = await base.ConfigureAsync(forceConfiguration);

            if (configured)
            {
                var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(this.Credentials!.UserName!, this.Credentials!.Password);

                var exception = InstallEapConfig(eapConfigWithCredentials);
                if (exception != null)
                {
                    configured = false;
                    messages = exception.Message.AsListItem();
                }
            }

            return (configured, messages);
        }

        public override async Task<(bool connected, IList<string> messages)> ConnectAsync()
        {
            var (connected, messages) = this.ValidateCredentials();

            if (!connected)
            {
                return (connected, messages);
            }

            Debug.Assert(
                !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                "Cannot configure EAP config that still needs credentials"
            );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable.AsListItem());
            }

            var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(this.Credentials!.UserName!, this.Credentials.Password.ToString()!);

            connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(eapConfigWithCredentials))
                {
                    message = Resource.ErrorConfiguredButUnableToConnect;
                }
                else
                {
                    // Hs2 is not enumerable
                    message = Resource.ErrorConfiguredButProbablyOutOfCoverage;
                }
            }

            return (connected, message.AsListItem());
        }
    }
}
