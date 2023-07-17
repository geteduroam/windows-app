using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class List : ICommand
    {
        public static string CommandName => "list";

        public static string CommandDescription => "Get a list of institutions or profiles for a institution.";

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption
            };

            command.SetHandler(async (string? institute) =>
            {
                if (!string.IsNullOrWhiteSpace(institute))
                {
                    await ShowProfilesAsync(institute);
                }
                else
                {
                    await ShowInstitutesAsync();
                }

            }, instituteOption);

            return command;
        }

        private static async Task ShowProfilesAsync(string? institute)
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
        }

        private static async Task ShowInstitutesAsync()
        {
            try
            {
                var getListTask = new GetInstitutesTask();
                var closestProviders = await getListTask.GetAsync();

                foreach (var provider in closestProviders)
                {
                    Console.WriteLine(provider.Name);
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

    }
}
