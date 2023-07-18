using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public async Task<bool> ConnectAsync()
        {
            return await Task.Run(ConnectToEduroam.TryToConnect);

        }
    }
}
