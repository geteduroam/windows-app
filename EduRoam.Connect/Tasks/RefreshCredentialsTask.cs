using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Store;
using EduRoam.Localization;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks
{
    public class RefreshCredentialsTask
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiParsingException"/>
        public async Task<string> RefreshAsync(bool force)
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
                await this.ReauthenticateAsync();
                return string.Empty;
            }

            return string.Empty;
        }

        public Task ReauthenticateAsync()
        {
            var identityProvider = RegistryStore.Instance.IdentityProvider;

            if (identityProvider?.ProfileId != null)
            {
                return this.HandleProfileSelectAsync(
                    identityProvider.Value.ProfileId,
                    identityProvider?.EapConfigXml,
                    skipOverview: true);
            }

            return Task.CompletedTask;
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

        /// <summary>
		/// downloads eap config based on profileId
		/// seperated into its own function as this can happen either through
		/// user selecting a profile or a profile being autoselected
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="eapConfigXml"></param>
		/// <param name="skipOverview"></param>
		/// <returns>True if function navigated somewhere</returns>
		/// <exception cref="XmlException">Parsing eap-config failed</exception>
        /// <exception cref="EduroamAppUserException"/>
		private async Task<bool> HandleProfileSelectAsync(string profileId, string? eapConfigXml, bool skipOverview = false)
        {
            EapConfig? eapConfig = null;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            if (!string.IsNullOrWhiteSpace(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(HandleProfileSelectAsync));

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }
            else
            {
                Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(HandleProfileSelectAsync));

                var eapConfigTask = new GetEapConfigTask();

                try
                {
                    eapConfig = await eapConfigTask.GetEapConfigAsync(profileId);
                }
                catch (UnknownProfileException)
                {
                    return false;
                }
            }

            // TODO: implement commented code below
            throw new NotImplementedException();

            //if (eapConfig != null)
            //{
            //    if (!CheckIfEapConfigIsSupported(eapConfig))
            //        return false;

            //    if (HasInfo(eapConfig) && !skipOverview)
            //    {
            //        LoadPageProfileOverview();
            //        return true;
            //    }
            //    if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
            //            .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
            //    {
            //        LoadPageCertificateOverview();
            //        return true;
            //    }

            //    LoadPageLogin();
            //    return true;
            //}
            //else if (!string.IsNullOrEmpty(profile?.redirect))
            //{
            //    // TODO: add option to go to selectmethod from redirect
            //    LoadPageRedirect(new Uri(profile.redirect));
            //    return true;
            //}
            //else if (profile?.oauth ?? false)
            //{

            //    LoadPageOAuthWait(profile);
            //    return true;
            //}
            //return false;
        }
    }
}
