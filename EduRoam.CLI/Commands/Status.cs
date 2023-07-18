using EduRoam.Connect;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Status : ICommand
    {
        public static string CommandName => "status";

        public static string CommandDescription => "Show status";

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                ShowStatus();

            });

            return command;
        }

        private static void ShowStatus()
        {
            Console.WriteLine();
            ConsoleExtension.WriteStatus("***********************************************");
            ConsoleExtension.WriteStatus("* ");
            ConsoleExtension.WriteStatus("***********************************************");
            Console.WriteLine();
        }
    }
}