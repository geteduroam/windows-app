using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Localization;

using System.Reflection;
using System.Xml;

namespace EduRoam.Connect.Tasks
{
    public class EapConfigTask
    {
        private readonly ManualResetEvent? mainThread;

        private readonly ManualResetEvent? cancelThread;

        public EapConfigTask()
        { }

        public EapConfigTask(ManualResetEvent mainThread, ManualResetEvent cancelThread)
        {
            this.mainThread = mainThread;
            this.cancelThread = cancelThread;
        }

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

            var getProfilesTask = new ProfilesTask();
            var profiles = await getProfilesTask.GetProfilesAsync(nameOfinstitute);
            var profile = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.InvariantCultureIgnoreCase));

            if (profile == null)
            {
                ConsoleExtension.WriteError($"Institute '{nameOfinstitute}' has no profile named '{profileName}'");
                throw new UnknownProfileException(nameOfinstitute, profileName);
            }

            return await this.ProcessProfileAsync(profile);

        }

        public static async Task<EapConfig?> GetEapConfigAsync(FileInfo eapConfigPath)
        {
            var filePath = eapConfigPath.FullName;
            var eapConfigContent = await File.ReadAllTextAsync(filePath);

            // create and return EapConfig object
            var eapConfig = EapConfig.FromXmlData(eapConfigContent);
            eapConfig.ProfileId = filePath;

            return eapConfig;
        }

        public static Task<EapConfig?> GetEapConfigAsync()
        {
            // create and return EapConfig object
            return LetsWifi.Instance.RequestAndDownloadEapConfig();
        }

        public async Task<EapConfig?> GetEapConfigAsync(string profileId)
        {
            var profile = await ProfilesTask.GetProfileAsync(profileId);

            if (profile == null)
            {
                throw new UnknownProfileException(profileId);
            }

            return await this.ProcessProfileAsync(profile);
        }

        public static bool IsEapConfigSupported(EapConfig eapConfig)
        {
            return EduRoamNetwork.IsEapConfigSupported(eapConfig);
        }

        private async Task<EapConfig?> ProcessProfileAsync(IdentityProviderProfile fullProfile)
        {
            var idpDownloader = new IdentityProviderDownloader();

            EapConfig? eapConfig;

            if (fullProfile.OAuth)
            {
                var oauthHandler = new OAuthHandler(fullProfile)
                {
                    MainThread = this.mainThread,
                    CancelThread = this.cancelThread
                };

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
            if (profile.OAuth || !string.IsNullOrEmpty(profile.Redirect))
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
                    string.Format(Resources.ErrorCannotConnectWithServer, e.Message));
            }
            catch (ApiParsingException e)
            {
                throw new EduroamAppUserException("xml parse exception",
                    string.Format(Resources.ErrorUnsupportedInstituteOrProfile, e.Message));
            }
        }

        /// <summary>
		/// Checks if an EAP-config file exists in the same folder as the executable.
		/// If the installed app and a EAP-config was bundled in a EXE using 7z, then this case will trigger
		/// </summary>
		/// <returns>EapConfig or null</returns>
		public static EapConfig? GetBundledEapConfig()
        {
            var appExeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (appExeLocation == null)
            {
                return null;
            }

            var files = Directory.GetFiles(appExeLocation, "*.eap-config");

            if (!files.Any())
            {
                return null;
            }

            try
            {
                var eapConfigContent = File.ReadAllText(files.First());
                var eapConfig = EapConfig.FromXmlData(eapConfigContent);

                return EduRoamNetwork.IsEapConfigSupported(eapConfig)
                    ? eapConfig
                    : null;
            }
            catch (XmlException) { }

            return null;
        }
    }
}
