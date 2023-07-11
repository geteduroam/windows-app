using EduRoam.Connect;
using EduRoam.Connect.Exceptions;


using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    public class ConnectInstitution : ICommand
    {
        public static string CommandName => "connect-institution";

        public static string CommandDescription => "connect-institution";

        private EapConfig? eapConfig;

        public Command Get()
        {
            var instituteOption = new Option<string>(
                name: "--i",
                description: "The name of the institute to connect to.");

            var profileOption = new Option<string?>(
                name: "--p",
                description: "Institute's profile to connect to.",
                getDefaultValue: () => null);

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption
            };

            command.SetHandler(async (institute, profile) =>
            {
                await ConnectProvider(institute, profile);
            }, instituteOption, profileOption);

            return command;
        }

        /// <summary>
		/// If no providers available try to download them
		/// </summary>
		private async Task ConnectProvider(string institute, string? profileName = null)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            try
            {
                await idpDownloader.LoadProviders(useGeodata: true);
                if (idpDownloader.Loaded)
                {
                    var provider = idpDownloader.Providers.SingleOrDefault(provider => provider.Name.Equals(institute, StringComparison.InvariantCultureIgnoreCase));

                    if (provider == null)
                    {
                        ConsoleExtension.WriteError($"Unknown institute '{institute}'");
                        return;
                    }
                    var profiles = idpDownloader.GetIdentityProviderProfiles(provider.Id);

                    string? profileId = null;
                    if (profileName != null)
                    {
                        var profile = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.InvariantCultureIgnoreCase));

                        if (profile == null)
                        {
                            ConsoleExtension.WriteError($"Institute '{institute}' has no profile named '{profileName}'");
                            return;
                        }

                        await this.ProcessProfileAsync(profile, idpDownloader);
                    }
                    else if (profiles.Count == 1) // skip the profile select and go with the first one
                    {
                        profileId = profiles.Single().Id;
                        if (!string.IsNullOrEmpty(profileId))
                        {
                            var fullProfile = idpDownloader.GetProfileFromId(profileId);
                            await this.ProcessProfileAsync(fullProfile, idpDownloader);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Institute '{institute}' has multiple profiles. Enter the name of a profile as an extra argument. (--p \"<profile name>\")");
                        Console.WriteLine();
                        Console.WriteLine("Profiles:");

                        foreach (var profile in profiles)
                        {
                            Console.WriteLine(profile.Name);
                        }
                    }
                }
            }
            catch (ApiParsingException e)
            {
                // Must never happen, because if the discovery is reached,
                // it must be parseable. Logging has been done upstream.
                ConsoleExtension.WriteError("API error");
                ConsoleExtension.WriteError(e.Message, e.GetType().ToString());                
            }
            catch (ApiUnreachableException)
            {
                ConsoleExtension.WriteError("No internet connection");
            }
            

        }

        private async Task ProcessProfileAsync(IdentityProviderProfile fullProfile, IdentityProviderDownloader idpDownloader)
        {
            try
            {
                if (fullProfile.oauth)
                {
                    var oauthHandler = new OAuthHandler(fullProfile);
                    await oauthHandler.Handle();

                    this.eapConfig = oauthHandler.EapConfig;
                }
                else
                {
                    this.eapConfig = await DownloadEapConfig(fullProfile, idpDownloader);
                }

                if (this.eapConfig == null)
                {
                    ConsoleExtension.WriteError("Profile is empty.");
                    return;
                }

                if (CheckIfEapConfigIsSupported(this.eapConfig))
                {
                    if (HasInfo(eapConfig))
                    {
                        ShowProfileOverview();
                    }
                    ResolveCertificates();

                    await this.ConnectAsync();
                }
            }
            catch (EduroamAppUserException ex) // TODO: catch this on some higher level
            {
                ConsoleExtension.WriteError(
                    ex.UserFacingMessage);
            }
            catch (Exception exc)
            {
                ConsoleExtension.WriteError(exc.ToString());
            }
        }

        private void ShowProfileOverview()
        {
            if (this.eapConfig?.InstitutionInfo != null) {
                var institutionInfo = this.eapConfig!.InstitutionInfo;

                Console.WriteLine();
                ConsoleExtension.WriteStatus("***********************************************");
                ConsoleExtension.WriteStatus($"* {institutionInfo.DisplayName}");
                ConsoleExtension.WriteStatus($"* {institutionInfo.Description}");
                if (!HasContactInfo(eapConfig.InstitutionInfo))
                {
                    ConsoleExtension.WriteStatusIf(institutionInfo.WebAddress != null, $"* {institutionInfo.WebAddress}");
                    ConsoleExtension.WriteStatusIf(institutionInfo.EmailAddress != null, $"* {institutionInfo.EmailAddress}");
                    ConsoleExtension.WriteStatusIf(institutionInfo.Phone != null, $"* {institutionInfo.Phone}");
                }
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            }
        }

        private void ResolveCertificates()
        {
            ConsoleExtension.WriteStatus("In order to continue the following certificates have to be installed.");
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            foreach (ConnectToEduroam.CertificateInstaller installer in installers)
            {
                Console.WriteLine();
                ConsoleExtension.WriteStatus($"* {installer.ToString()}, installed: {(installer.IsInstalled ? "✓" : "x")}");
                Console.WriteLine();
            }

            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);

            if (certificatesNotInstalled.Any())
            {
                ConsoleExtension.WriteStatus("One or more certificates are not installed yet. Install the certificates? (y/N)");
                var key = Console.ReadKey();

                if (key.KeyChar != 'y' && key.KeyChar != 'Y')
                {
                    ConsoleExtension.WriteError("Cannot connect when not all required certificates are stored");
                    return;
                }

                foreach (var installer in certificatesNotInstalled)
                {
                    installer.AttemptInstallCertificate();
                }
            }
         }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        /// <exception cref="EduroamAppUserException">description</exception>
        public async Task<EapConfig?> DownloadEapConfig(IdentityProviderProfile profile, IdentityProviderDownloader idpDownloader)
        {
            if (string.IsNullOrEmpty(profile?.Id))
                return null;

            // if OAuth
            if (profile.oauth || !string.IsNullOrEmpty(profile.redirect))
                return null;

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
            bool hasWebAddress = !string.IsNullOrEmpty(info.WebAddress);
            bool hasEmailAddress = !string.IsNullOrEmpty(info.EmailAddress);
            bool hasPhone = !string.IsNullOrEmpty(info.Phone);
            return (hasWebAddress || hasEmailAddress || hasPhone);
        }

        /// <summary>
		/// Tries to connect to eduroam
		/// </summary>
		/// <returns></returns>
		public async Task ConnectAsync()
        {
            bool connected = await Task.Run(ConnectToEduroam.TryToConnect);
            
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
