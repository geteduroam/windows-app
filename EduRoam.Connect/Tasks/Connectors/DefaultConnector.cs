using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;

using System.Diagnostics;

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
                var exception = InstallEapConfig(this.eapConfig);
                if (exception != null)
                {
                    configured = false;
                    messages = exception.Message.AsListItem();
                }
            }

            return (configured, messages);
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public override async Task<(bool connected, IList<string> messages)> ConnectAsync()
        {
            Debug.Assert(
                    !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                    "Cannot configure EAP config that still needs credentials"
                );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable.AsListItem());
            }

            foreach (var authMethod in this.eapConfig.SupportedAuthenticationMethods)
            {
                var authMethodInstaller = new EapAuthMethodInstaller(authMethod);

                // check if we need to wait for the certificate to become valid
                var certValid = authMethodInstaller.GetTimeWhenValid().From;
                if (DateTime.Now <= certValid)
                {
                    // dispatch the event which creates the clock the end user sees
                    return (false, Resource.ErrorClientCredentialNotValidYes.AsListItem());
                }
            }

            var connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                if (this.eapConfig == null)
                {
                    message = Resource.ErrorConfiguredButNotConnected;

                }
                else if (EduRoamNetwork.IsNetworkInRange(this.eapConfig))
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
