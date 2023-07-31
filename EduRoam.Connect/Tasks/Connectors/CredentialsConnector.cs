using EduRoam.Connect.Eap;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Language;

using System.Diagnostics;
using System.Security;

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

        public async Task<(bool connected, string message)> ConnectAsync(string userName, SecureString password)
        {
            var connected = false;
            var message = string.Empty;

            Debug.Assert(
                !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                "Cannot configure EAP config that still needs credentials"
            );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (connected, Resource.ErrorWirelessUnavailable);
            }

            var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(userName, password.ToString()!);

            connected = await Task.Run(ConnectToEduroam.TryToConnect);

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(this.eapConfig))
                {
                    message = Resource.ErrorConfiguredButUnableToConnect;
                }
                else
                {
                    // Hs2 is not enumerable
                    message = Resource.ErrorConfiguredButProbablyOutOfCoverage;
                }
            }

            return (connected, message);
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
                var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(this.Credentials!.UserName!, this.Credentials!.Password.ToString()!);

                var exception = this.InstallEapConfig(eapConfigWithCredentials);
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
