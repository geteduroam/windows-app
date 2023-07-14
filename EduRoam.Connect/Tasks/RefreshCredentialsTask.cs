using EduRoam.Connect.Exceptions;

using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace EduRoam.Connect.Tasks
{
    public class RefreshCredentialsTask
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiParsingException"/>
        public async Task<string> Refresh()
        {
            var response = LetsWifi.RefreshResponse.Failed;

            try
            {
                response = await LetsWifi.RefreshAndInstallEapConfig(force: true, onlyLetsWifi: true);
            }
            catch (HttpRequestException)
            {
                this.Reauthenticate();
                return string.Empty;
            }

            switch (response)
            {
                case LetsWifi.RefreshResponse.Success:
                case LetsWifi.RefreshResponse.UpdatedEapXml: // Should never happen due to onlyLetsWifi=true
                    return this.GetExpirationInfo();
                case LetsWifi.RefreshResponse.StillValid: // should never happend due to force=true
                case LetsWifi.RefreshResponse.AccessDenied:
                case LetsWifi.RefreshResponse.NewRootCaRequired:
                case LetsWifi.RefreshResponse.NotRefreshable:
                case LetsWifi.RefreshResponse.Failed:
                    break;
            }

            return string.Empty;
        }

        private void Reauthenticate()
        {
            if (PersistingStore.IdentityProvider?.ProfileId != null)
            {
                _ = this.HandleProfileSelect(
                    PersistingStore.IdentityProvider.Value.ProfileId,
                    PersistingStore.IdentityProvider?.EapConfigXml,
                    skipOverview: true);
            }
        }

        /// <summary>
		/// Loads info regarding the certficate of the persising store and displays it to the usr
		/// </summary>
		private string GetExpirationInfo()
        {
            var message = new StringBuilder();

            if (PersistingStore.IdentityProvider?.NotAfter != null)
            {
                var expireDate = PersistingStore.IdentityProvider.Value.NotAfter;
                var nowDate = DateTime.Now;
                var diffDate = expireDate - nowDate;

                message.Append("Your account is valid for");
                message.AppendLine(expireDate?.ToString(CultureInfo.InvariantCulture));

                if (diffDate.Value.Days > 1)
                {
                    message.AppendLine(diffDate.Value.Days.ToString(CultureInfo.InvariantCulture) + " more days");
                }
                else if (diffDate.Value.Hours > 1)
                {
                    message.Append(diffDate.Value.Hours.ToString(CultureInfo.InvariantCulture) + " more hours");
                }
                else
                {
                    message.Append(diffDate.Value.Minutes.ToString(CultureInfo.InvariantCulture) + " more minutes");
                }
            }

            return message.ToString();
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
		private async Task<bool> HandleProfileSelect(string profileId, string? eapConfigXml, bool skipOverview = false)
        {
            EapConfig? eapConfig = null;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            if (!string.IsNullOrEmpty(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(HandleProfileSelect));

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }
            else
            {
                Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(HandleProfileSelect));

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
