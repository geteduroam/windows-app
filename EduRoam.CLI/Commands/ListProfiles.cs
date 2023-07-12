using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class ListProfiles : ICommand
    {
        public static string CommandName => "list-profiles";

        public static string CommandDescription => "Get a list of institution profiles";

        public Command GetCommand()
        {
            var instituteOption = new Option<string>(
                name: "--i",
                parseArgument: OptionExtensions.NonEmptyString,
                isDefault: true,
                description: "The name of the institute to connect to.");

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption
            };


            command.SetHandler(async (string institute) =>
            {
                var getProfilesTask = new GetProfilesTask();

                try
                {
                    var profiles = await getProfilesTask.GetProfilesAsync(institute);

                    foreach (var profile in profiles)
                    {
                        Console.WriteLine(profile.Name);
                    }
                }
                catch (Exception exc) when (exc is UnknownInstituteException || exc is UnknownProfileException)
                {
                    ConsoleExtension.WriteError(exc.Message);
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

            }, instituteOption);

            return command;
        }


    }
}
