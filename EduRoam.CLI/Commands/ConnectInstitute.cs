using EduRoam.Connect;
using EduRoam.Connect.Exceptions;


using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

                    string? autoProfileId = null;
                    if (profileName != null)
                    {
                        var profile = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.InvariantCultureIgnoreCase));

                        if (profile == null)
                        {
                            ConsoleExtension.WriteError($"Institute '{institute}' has no profile named '{profileName}'");
                            return;
                        }


                    }
                    else if (profiles.Count == 1) // skip the profile select and go with the first one
                    {
                        autoProfileId = profiles.FirstOrDefault().Id;
                        if (!string.IsNullOrEmpty(autoProfileId))
                        {
                            var fullProfile = idpDownloader.GetProfileFromId(autoProfileId);
                            try
                            {
                                if (fullProfile.oauth)
                                {
                                    var oauthHandler = new OAuthHandler(fullProfile);
                                    await oauthHandler.Handle();

                                    this.eapConfig = oauthHandler.EapConfig;
                                } else
                                {
                                    var eapConfig = await DownloadEapConfig(fullProfile, idpDownloader);
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
                    }
                    else
                    {
                        Console.WriteLine($"Institute '{institute}' has multiple profiles. Enter the name of a profile as an extra argument.");
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

        
    }
}
