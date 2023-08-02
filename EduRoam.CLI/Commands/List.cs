using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class List : ICommand
    {
        public static readonly string CommandName = "list";

        public static readonly string CommandDescription = Resources.CommandDescriptionList;

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);
            var queryOption = Options.GetQueryOption();

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                queryOption
            };

            command.SetHandler(async (string? institute, string? query) =>
            {
                if (!string.IsNullOrWhiteSpace(institute))
                {
                    await ShowProfilesAsync(institute, query);
                }
                else
                {
                    await ShowInstitutesAsync(query);
                }

            }, instituteOption, queryOption);

            return command;
        }

        private static async Task ShowProfilesAsync(string institute, string? query = null)
        {
            var getProfilesTask = new GetProfilesTask();

            try
            {
                var profiles = await getProfilesTask.GetProfilesAsync(institute, query);

                if (profiles.Any())
                {
                    foreach (var profile in profiles)
                    {
                        Console.WriteLine(profile.Name);
                    }
                }
                else
                {
                    ConsoleExtension.WriteWarning(Resources.WarningNoProfilesFound);
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
                ConsoleExtension.WriteError(Resources.ErrorApi);
                ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
            }
            catch (ApiUnreachableException)
            {
                ConsoleExtension.WriteError(Resources.ErrorNoInternet);
            }
        }

        private static async Task ShowInstitutesAsync(string? query)
        {
            try
            {
                var getListTask = new GetInstitutesTask();
                var institutes = await getListTask.GetAsync(query);

                if (institutes.Any())
                {
                    foreach (var provider in institutes)
                    {
                        Console.WriteLine(provider.Name);
                    }
                }
                else
                {
                    ConsoleExtension.WriteWarning(Resources.WarningNoInstitutesFound);
                }
            }
            catch (ApiParsingException e)
            {
                // Must never happen, because if the discovery is reached,
                // it must be parseable. Logging has been done upstream.
                ConsoleExtension.WriteError(Resources.ErrorApi);
                ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
            }
            catch (ApiUnreachableException)
            {
                ConsoleExtension.WriteError(Resources.ErrorNoInternet);
            }
        }
    }
}
