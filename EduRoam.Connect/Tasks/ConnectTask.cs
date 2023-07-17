using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="eapConfig"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        public async Task<bool> ConnectAsync()
        {
            return await this.TryToConnectAsync();

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
                    //if (EduRoamNetwork.IsNetworkInRange(this.eapConfig!))
                    //{
                    //    ConsoleExtension.WriteError("Everything is configured!\nUnable to connect to eduroam.");
                    //}
                    //else
                    //{
                    //     Hs2 is not enumerable
                    ConsoleExtension.WriteError("Everything is configured!\nUnable to connect to eduroam, you're probably out of coverage.");
                    //}
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
