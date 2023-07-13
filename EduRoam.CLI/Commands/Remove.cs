using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public readonly static string CommandName = "remove";

        public static string CommandDescription => "Remove configured Wi-Fi profile and/or root certificates";

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
