using EduRoam.Connect;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Status : ICommand
    {
        public static readonly string CommandName = "status";

        public static readonly string CommandDescription = Resource.CommandDescriptionStatus;

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
                ConsoleExtension.WriteStatus($"* {Resource.LabelProfile}: {status.ProfileName}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.LabelAccountValidFor}: {status.TimeLeft}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.LabelHelp}: {Resource.HelpUrl}");
                ConsoleExtension.WriteStatus($"* {Resource.LabelVersion}: {status.Version}");
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            });

            return command;
        }
    }
}