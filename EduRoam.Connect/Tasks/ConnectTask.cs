using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public async Task<(bool connected, string message)> ConnectAsync()
        {
            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable);
            }

            var connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                var eapConfigTask = new GetEapConfigTask();

                var eapConfig = await eapConfigTask.GetEapConfigAsync();

                if (eapConfig == null)
                {
                    message = Resource.ConfiguredButNotConnected;

                }
                else if (EduRoamNetwork.IsNetworkInRange(eapConfig))
                {
                    message = Resource.ConfiguredButUnableToConnect;
                }
                else
                {
                    // Hs2 is not enumerable
                    message = Resource.ConfiguredButProbablyOutOfCoverage;
                }
            }

            return (connected, message);
        }
    }
}
