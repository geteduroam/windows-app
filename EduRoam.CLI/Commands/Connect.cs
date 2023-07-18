using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static string CommandName => "connect";

        public static string CommandDescription => "Connect with the current profile";

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);
            var profileOption = Options.GetProfileOption(optional: true);
            var eapConfigFileOption = Options.GetEapConfigOption();
            var forceOption = Options.GetForceOption();

            var command = new Command(CommandName, CommandDescription)
            {
                eapConfigFileOption,
                instituteOption,
                profileOption,
                forceOption
            };

            command.AddValidator(validator =>
            {
                var instituteOptionValue = validator.GetValueForOption(instituteOption);
                var profileOptionValue = validator.GetValueForOption(profileOption);
                var eapConfigFileArgValue = validator.GetValueForOption(eapConfigFileOption);

                if (eapConfigFileArgValue == null && (string.IsNullOrWhiteSpace(instituteOptionValue) || string.IsNullOrWhiteSpace(profileOptionValue)))
                {
                    validator.ErrorMessage = $"Missing options. Provide the {eapConfigFileOption.Aliases.First()} option or both {instituteOption.Name} and {profileOption.Name} options";
                }
            });

            command.SetHandler(async (FileInfo? eapConfigFile, string? institute, string? profileName, bool force) =>
            {
                var connectTask = new ConnectTask();

                try
                {
                    var connected = await connectTask.ConnectAsync();

                    try
                    {
                        if (connected)
                        {
                            ConsoleExtension.WriteError("You are now connected to EduRoam.");
                        }
                        else
                        {
                            var eapConfigTask = new GetEapConfigTask();

                            var eapConfig = await eapConfigTask.GetEapConfigAsync();

                            if (eapConfig == null)
                            {
                                ConsoleExtension.WriteError($"Everything is configured!\nCould not connect because no EAP Config could be found");

                            }
                            else if (EduRoamNetwork.IsNetworkInRange(eapConfig))
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
                catch (Exception exc) when (exc is ArgumentException || exc is UnknownInstituteException || exc is UnknownProfileException)
                {
                    ConsoleExtension.WriteError(exc.Message);
                }
                catch (EduroamAppUserException ex)
                {
                    ConsoleExtension.WriteError(
                        ex.UserFacingMessage);
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

            }, eapConfigFileOption, instituteOption, profileOption, forceOption);

            return command;
        }
    }
}
