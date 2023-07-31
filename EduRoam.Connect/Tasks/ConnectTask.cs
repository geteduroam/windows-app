using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks.Connectors;

using System.Security;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        public async Task<Connector?> GetConnectorAsync()
        {
            var eapConfig = await GetEapConfig();

            return Connector.GetInstance(eapConfig);
        }

        public async Task<(bool, IList<string>)> ValidateCredentialsAsync(string? userName, SecureString password)
        {
            if (string.IsNullOrWhiteSpace(userName) || password.Length == 0)
            {
                return (false, Resource.ErrorInvalidCredentials.AsListItem());
            }

            var eapConfig = await GetEapConfig();
            if (eapConfig == null)
            {
                // this should never happen, because this method should only be called after a connection type is determined based upon GetConnectionTypeAsync().
                return (false, Resource.ErrorConfiguredButNotConnected.AsListItem());
            }

            var (realm, hint) = eapConfig.GetClientInnerIdentityRestrictions();

            var brokenRules = IdentityProviderParser.GetRulesBrokenOnUsername(userName, realm, hint);

            if (brokenRules.Any())
            {
                return (false, brokenRules.ToList());
            }

            return (true, Array.Empty<string>());
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public async Task<(bool connected, IList<string> messages)> ConnectAsync()
        {
            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable.AsListItem());
            }

            var connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                var eapConfig = await GetEapConfig();
                if (eapConfig == null)
                {
                    message = Resource.ErrorConfiguredButNotConnected;

                }
                else if (EduRoamNetwork.IsNetworkInRange(eapConfig))
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

        public Task<(bool connected, string message)> ConnectAsync(string? userName, SecureString password)
        {
            throw new NotImplementedException();
        }


        private static async Task<EapConfig?> GetEapConfig()
        {
            var eapConfigTask = new GetEapConfigTask();

            return await eapConfigTask.GetEapConfigAsync();
        }
    }
}
