using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Uninstall : ICommand
    {
        public readonly static string CommandName = "uninstall";

        public static string CommandDescription => "Uninstall application";

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
