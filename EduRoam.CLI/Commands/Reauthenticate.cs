using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Reauthenticate : ICommand
    {
        public static readonly string CommandName = "reauthenticate";

        public static readonly string CommandDescription = Resources.CommandDescriptionReauthenticate;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(async () =>
            {
                var task = new RefreshCredentialsTask();
                await task.ReauthenticateAsync();

                Console.WriteLine(Resources.Done);

            });

            return command;
        }
    }
}
