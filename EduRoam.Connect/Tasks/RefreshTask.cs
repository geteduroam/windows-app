using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Store;
using EduRoam.Localization;

namespace EduRoam.Connect.Tasks
{
    public class RefreshTask
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiParsingException"/>
        public static async Task<string> RefreshAsync(bool force)
        {
            try
            {
                var response = await LetsWifi.Instance.RefreshAndInstallEapConfig(force, onlyLetsWifi: true);

                switch (response)
                {
                    case RefreshResponse.Success:
                    case RefreshResponse.UpdatedEapXml: // Should never happen due to onlyLetsWifi=true
                        return GetExpirationInfo();
                    case RefreshResponse.StillValid: // should never happend due to force=true
                    case RefreshResponse.AccessDenied:
                    case RefreshResponse.NewRootCaRequired:
                    case RefreshResponse.NotRefreshable:
                    case RefreshResponse.Failed:
                        break;
                }

            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
		/// Loads info regarding the certficate of the persising store and displays it to the usr
		/// </summary>
		private static string GetExpirationInfo()
        {
            if (RegistryStore.Instance.IdentityProvider?.NotAfter != null)
            {
                var statusTask = new StatusTask();
                var status = statusTask.GetStatus();

                return $"{Resources.LabelAccountValidFor}: {status.TimeLeft}";

            }

            return string.Empty;
        }
    }
}
