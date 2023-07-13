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

            command.SetHandler(() =>
            {
                throw new NotSupportedException("Not supported yet");
            });

            return command;
        }

    }
}
