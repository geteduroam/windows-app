using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class ConnectByProfile : ICommand
    {
        public static string CommandName => "connect";

        public static string CommandDescription => "Configure Wi-Fi based on a institution profile and connect ";

        public Command GetCommand()
        {
            var instituteOption = new Option<string>(
                name: "--i",
                parseArgument: OptionExtensions.NonEmptyString,
                isDefault: true,
                description: "The name of the institute to connect to.");

            var profileOption = new Option<string>(
                name: "--p",
                parseArgument: OptionExtensions.NonEmptyString,
                isDefault: true,
                description: "Institute's profile to connect to.");

            var forceOption = new Option<bool>(
                name: "--f",
                description: "Force automatic configuration if the profile is not already configured (fully).",
                getDefaultValue: () => false);

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption,
                forceOption
            };


            command.SetHandler(async (string institute, string profileName, bool force) =>
            {
                var getEapConfig = new GetEapConfigTask();
                var eapConfig = await getEapConfig.GetEapConfigAsync(institute, profileName);

                if (eapConfig == null)
                {
                    ConsoleExtension.WriteError($"Could not connect, EAP Config is empty");
                    return;
                }

                var connectTask = new ConnectTask(eapConfig);

                try
                {
                    var connected = await connectTask.ConnectAsync(force);

                    try
                    {
                        if (connected)
                        {
                            ConsoleExtension.WriteError("You are now connected to EduRoam.");
                        }
                        else
                        {
                            if (EduRoamNetwork.IsNetworkInRange(eapConfig))
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

            }, instituteOption, profileOption, forceOption);

            return command;
        }
    }
}
