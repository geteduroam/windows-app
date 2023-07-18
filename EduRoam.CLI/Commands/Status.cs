using EduRoam.Connect;
using EduRoam.Connect.Tasks;

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
                var statusTask = new StatusTask();
                var status = statusTask.GetStatus();

                Console.WriteLine();
                ConsoleExtension.WriteStatus("***********************************************");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.ProfileLabel}: {status.ProfileName}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.AccountValidFor}: {status.TimeLeft}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.VersionLabel}: {status.Version}");
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            });

            return command;
        }


    }
}