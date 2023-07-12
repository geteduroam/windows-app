using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class ConnectTask
    {
        private EapConfig? eapConfig;

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <exception cref="ArgumentException"
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        /// <exception cref="UnknownProfileException" />
        /// <exception cref="EduroamAppUserException"/>
        public async Task ConnectAsync(string institute, string profileName, bool forceConfiguration = false)
        {
            if (string.IsNullOrWhiteSpace(institute))
            {
                throw new ArgumentException("Empty institute", nameof(institute));
            }
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Empty profile", nameof(profileName));
            }

            var getProfilesTask = new GetProfilesTask();
            var profiles = await getProfilesTask.GetProfilesAsync(institute);
            var profile = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.InvariantCultureIgnoreCase));

            if (profile == null)
            {
                ConsoleExtension.WriteError($"Institute '{institute}' has no profile named '{profileName}'");
                throw new UnknownProfileException(institute, profileName);
            }

            await this.ProcessProfileAsync(profile, forceConfiguration);

        }

        private async Task ProcessProfileAsync(IdentityProviderProfile fullProfile, bool forceConfiguration)
        {
            var idpDownloader = new IdentityProviderDownloader();


            if (fullProfile.oauth)
            {
                var oauthHandler = new OAuthHandler(fullProfile);
                await oauthHandler.Handle();

                this.eapConfig = oauthHandler.EapConfig;
            }
            else
            {
                this.eapConfig = await DownloadEapConfigAsync(fullProfile, idpDownloader);
            }

            if (this.eapConfig == null)
            {
                ConsoleExtension.WriteError("Profile is empty.");
                return;
            }

            if (CheckIfEapConfigIsSupported(this.eapConfig))
            {
                if (HasInfo(this.eapConfig))
                {
                    this.ShowProfileOverview();
                }
                this.ResolveCertificates(forceConfiguration);

                await this.ConnectAsync();
            }
        }


        private void ShowProfileOverview()
        {
            if (this.eapConfig?.InstitutionInfo != null)
            {
                var institutionInfo = this.eapConfig!.InstitutionInfo;

                Console.WriteLine();
                ConsoleExtension.WriteStatus("***********************************************");
                ConsoleExtension.WriteStatus($"* {institutionInfo.DisplayName}");
                ConsoleExtension.WriteStatus($"* {institutionInfo.Description}");
                if (!HasContactInfo(this.eapConfig.InstitutionInfo))
                {
                    ConsoleExtension.WriteStatusIf(institutionInfo.WebAddress != null, $"* {institutionInfo.WebAddress}");
                    ConsoleExtension.WriteStatusIf(institutionInfo.EmailAddress != null, $"* {institutionInfo.EmailAddress}");
                    ConsoleExtension.WriteStatusIf(institutionInfo.Phone != null, $"* {institutionInfo.Phone}");
                }
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            }
        }

        private void ResolveCertificates(bool forceConfiguration)
        {
            ConsoleExtension.WriteStatus("In order to continue the following certificates have to be installed.");
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            foreach (var installer in installers)
            {
                Console.WriteLine();
                ConsoleExtension.WriteStatus($"* {installer}, installed: {(installer.IsInstalled ? "✓" : "x")}");
                Console.WriteLine();
            }

            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);

            if (certificatesNotInstalled.Any())
            {
                if (!forceConfiguration)
                {
                    ConsoleExtension.WriteStatus("One or more certificates are not installed yet. Install the certificates? (y/N)");
                }
                else
                {
                    foreach (var installer in certificatesNotInstalled)
                    {
                        installer.AttemptInstallCertificate();
                    }
                }
            }
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

        private static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Used to determine if an eapconfig has enough info
        /// for the ProfileOverview page to show
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static bool HasInfo(EapConfig config)
            => !string.IsNullOrEmpty(config.InstitutionInfo.WebAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.EmailAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Description)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Phone)
            || !string.IsNullOrEmpty(config.InstitutionInfo.TermsOfUse);

        private static bool HasContactInfo(EapConfig.ProviderInfo info)
        {
            var hasWebAddress = !string.IsNullOrEmpty(info.WebAddress);
            var hasEmailAddress = !string.IsNullOrEmpty(info.EmailAddress);
            var hasPhone = !string.IsNullOrEmpty(info.Phone);
            return (hasWebAddress || hasEmailAddress || hasPhone);
        }

        /// <summary>
        /// Tries to connect to eduroam
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            var connected = await Task.Run(ConnectToEduroam.TryToConnect);

            try
            {
                if (connected)
                {
                    ConsoleExtension.WriteError("You are now connected to EduRoam.");
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
            }
            catch (EduroamAppUserException ex)
            {
                // NICE TO HAVE: log the error
                ConsoleExtension.WriteError($"Could not connect. \nException: {ex.UserFacingMessage}.");
            }

        }
    }
}
