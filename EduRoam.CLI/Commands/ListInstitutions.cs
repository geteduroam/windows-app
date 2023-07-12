using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class ListInstitutions : ICommand
    {
        public static string CommandName => "list-institutions";

        public static string CommandDescription => "Get a list of institutions with a EduRoam profile";

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(async () =>
            {
                await ShowProvidersAsync();
            });

            return command;
        }

        private static async Task ShowProvidersAsync()
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
