using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Refresh : ICommand
    {
        public readonly static string CommandName = "refresh";

        public static string CommandDescription => "Refresh credential";

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription)
            {

            };

            command.SetHandler(async () =>
            {
                var task = new RefreshCredentialsTask();
                var message = await task.RefreshAsync();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine(message);
                }
            });

            return command;
        }

    }
}
