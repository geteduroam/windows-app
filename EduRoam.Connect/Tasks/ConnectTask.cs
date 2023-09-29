using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        public static async Task<Connector?> GetConnectorAsync()
        {
            var eapConfig = await EapConfigTask.GetEapConfigAsync();

            return Connector.GetInstance(eapConfig);
        }

        public static async Task<TaskStatus> ValidateCredentialsAsync(string? userName, SecureString password)
        {
            if (string.IsNullOrWhiteSpace(userName) || password.Length == 0)
            {
                return TaskStatus.AsFailure(Resources.ErrorInvalidCredentials);
            }

            var eapConfig = await EapConfigTask.GetEapConfigAsync();
            if (eapConfig == null)
            {
                // this should never happen, because this method should only be called after a connection type is determined based upon GetConnectionTypeAsync().
                return TaskStatus.AsFailure(Resources.ErrorConfiguredButNotConnected);
            }

            var (realm, hint) = eapConfig.GetClientInnerIdentityRestrictions();

            var brokenRules = IdentityProviderParser.GetRulesBrokenOnUsername(userName, realm, hint);

            if (brokenRules.Any())
            {
                return TaskStatus.AsFailure(brokenRules.ToArray());
            }

            return TaskStatus.AsSuccess();
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public static async Task<TaskStatus> ConnectAsync()
        {
            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return TaskStatus.AsFailure(Resources.ErrorWirelessUnavailable);
            }

            var status = new TaskStatus()
            {
                Success = await Task.Run(ConnectToEduroam.TryToConnect)
            };

            if (status.Success)
            {
                status.Messages.Add(ApplicationResources.GetString("Connected"));
            }
            else
            {
                var eapConfig = await EapConfigTask.GetEapConfigAsync();
                if (eapConfig == null)
                {
                    status.Errors.Add(Resources.ErrorConfiguredButNotConnected);

                }
                else if (EduRoamNetwork.IsNetworkInRange(eapConfig))
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
