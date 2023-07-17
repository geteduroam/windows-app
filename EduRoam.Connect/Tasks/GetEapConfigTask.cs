using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class GetEapConfigTask
    {
        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="nameOfinstitute"></param>
        /// <param name="profileName"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        /// <exception cref="ArgumentException"
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        /// <exception cref="UnknownProfileException" />
        /// <exception cref="EduroamAppUserException"/>
        public async Task<EapConfig?> GetEapConfigAsync(string nameOfinstitute, string profileName)
        {
            if (string.IsNullOrWhiteSpace(nameOfinstitute))
            {
                throw new ArgumentNullException(nameof(nameOfinstitute));
            }
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentNullException(nameof(profileName));
            }

            var getProfilesTask = new GetProfilesTask();
            var profiles = await getProfilesTask.GetProfilesAsync(nameOfinstitute);
            var profile = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.InvariantCultureIgnoreCase));

            if (profile == null)
            {
                ConsoleExtension.WriteError($"Institute '{nameOfinstitute}' has no profile named '{profileName}'");
                throw new UnknownProfileException(nameOfinstitute, profileName);
            }

            return await ProcessProfileAsync(profile);

        }

        public async Task<EapConfig?> GetEapConfigAsync(FileInfo eapConfigPath)
        {
            var filePath = eapConfigPath.FullName;
            var eapConfigContent = await File.ReadAllTextAsync(filePath);

            // create and return EapConfig object
            var eapConfig = EapConfig.FromXmlData(eapConfigContent);
            eapConfig.ProfileId = filePath;

            return eapConfig;
        }

        public async Task<EapConfig?> GetEapConfigAsync()
        {
            // create and return EapConfig object
            var eapConfig = await LetsWifi.Instance.RequestAndDownloadEapConfig();

            return eapConfig;
        }

        public async Task<EapConfig?> GetEapConfigAsync(string profileId)
        {
            var getProfilesTask = new GetProfilesTask();
            var profile = getProfilesTask.GetProfile(profileId);

            if (profile == null)
            {
                throw new UnknownProfileException(profileId);
            }

            return await ProcessProfileAsync(profile);
        }

        private static async Task<EapConfig?> ProcessProfileAsync(IdentityProviderProfile fullProfile)
        {
            var idpDownloader = new IdentityProviderDownloader();

            EapConfig? eapConfig;

            if (fullProfile.oauth)
            {
                var oauthHandler = new OAuthHandler(fullProfile);
                await oauthHandler.Handle();

                eapConfig = oauthHandler.EapConfig;
            }
            else
            {
                eapConfig = await DownloadEapConfigAsync(fullProfile, idpDownloader);
            }

            return eapConfig;
        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        /// <exception cref="EduroamAppUserException">description</exception>
        private static async Task<EapConfig?> DownloadEapConfigAsync(IdentityProviderProfile profile, IdentityProviderDownloader idpDownloader)
        {
            if (string.IsNullOrEmpty(profile?.Id))
            {
                return null;
            }

            // if OAuth
            if (profile.oauth || !string.IsNullOrEmpty(profile.redirect))
            {
                return null;
            }

            try
            {
                return await Task.Run(()
                    => idpDownloader.DownloadEapConfig(profile.Id)
                );
            }
            catch (ApiUnreachableException e)
            {
                throw new EduroamAppUserException("HttpRequestException",
                    "Couldn't connect to the server.\n\n" +
                    "Make sure that you are connected to the internet, then try again.\n" +
                    "Exception: " + e.Message);
            }
            catch (ApiParsingException e)
            {
                throw new EduroamAppUserException("xml parse exception",
                    "The institution or profile is either not supported or malformed. " +
                    "Please select a different institution or profile.\n\n" +
                    "Exception: " + e.Message);
            }
        }

    }
}
