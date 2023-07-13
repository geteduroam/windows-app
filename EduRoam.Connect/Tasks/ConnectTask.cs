using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        private EapConfig eapConfig;

        public ConnectTask(EapConfig eapConfig)
        {
            this.eapConfig = eapConfig;
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="eapConfig"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        public async Task<bool> ConnectAsync(bool forceConfiguration = false)
        {
            if (this.eapConfig == null)
            {
                return false;
            }

            if (!this.CheckIfEapConfigIsSupported())
            {
                return false;
            }

            var resolveConfiguration = new ResolveConfigurationTask();
            var configurationReady = resolveConfiguration.ResolveConfiguration(this.eapConfig, forceConfiguration);

            if (!configurationReady)
            {
                return false;
            }

            return await this.TryToConnectAsync();

        }

        private bool CheckIfEapConfigIsSupported()
        {
            if (!EduRoamNetwork.IsEapConfigSupported(this.eapConfig!))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to connect to eduroam
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TryToConnectAsync()
        {
            try
            {
                var connected = await Task.Run(ConnectToEduroam.TryToConnect);

                if (connected)
                {
                    ConsoleExtension.WriteStatus("You are now connected to EduRoam.");
                }
                else
                {
                    if (EduRoamNetwork.IsNetworkInRange(this.eapConfig!))
                    {
                        ConsoleExtension.WriteError("Everything is configured!\nUnable to connect to eduroam.");
                    }
                    else
                    {
                        // Hs2 is not enumerable
                        ConsoleExtension.WriteError("Everything is configured!\nUnable to connect to eduroam, you're probably out of coverage.");
                    }
                }

                return connected;
            }
            catch (EduroamAppUserException ex)
            {
                // NICE TO HAVE: log the error
                ConsoleExtension.WriteError($"Could not connect. \nException: {ex.UserFacingMessage}.");
                return false;
            }


        }
    }
}
