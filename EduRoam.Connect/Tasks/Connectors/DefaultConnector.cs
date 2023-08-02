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

        public override async Task<TaskStatus> ConfigureAsync(bool forceConfiguration = false)
        {
            var status = await base.ConfigureAsync(forceConfiguration);

            if (status.Success)
            {
                var exception = InstallEapConfig(this.eapConfig);
                if (exception != null)
                {
                    status.Success = false;
                    status.Errors.Add(exception.Message);
                }
            }

            return status;
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public override async Task<TaskStatus> ConnectAsync()
        {
            var status = TaskStatus.AsFailure();

            Debug.Assert(
                    !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                    "Cannot configure EAP config that still needs credentials"
                );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                status.Errors.Add(Resources.ErrorWirelessUnavailable);
                return status;
            }

            foreach (var authMethod in this.eapConfig.SupportedAuthenticationMethods)
            {
                var authMethodInstaller = new EapAuthMethodInstaller(authMethod);

                // check if we need to wait for the certificate to become valid
                var certValid = authMethodInstaller.GetTimeWhenValid().From;
                if (DateTime.Now <= certValid)
                {
                    // dispatch the event which creates the clock the end user sees
                    status.Errors.Add(Resources.ErrorClientCredentialNotValidYes);
                    return status;
                }
            }

            status.Success = await Task.Run(ConnectToEduroam.TryToConnect);

            if (status.Success)
            {
                status.Messages.Add(Resources.Connected);
            }
            else
            {
                if (this.eapConfig == null)
                {
                    status.Errors.Add(Resources.ErrorConfiguredButNotConnected);

                }
                else if (EduRoamNetwork.IsNetworkInRange(this.eapConfig))
                {
                    status.Errors.Add(Resources.ErrorConfiguredButUnableToConnect);
                }
                else
                {
                    // Hs2 is not enumerable
                    status.Errors.Add(Resources.ErrorConfiguredButProbablyOutOfCoverage);
                }
            }

            return status;
        }

    }
}
