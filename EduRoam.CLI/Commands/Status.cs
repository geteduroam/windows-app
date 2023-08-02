using EduRoam.Connect;
using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Status : ICommand
    {
        public static readonly string CommandName = "status";

        public static readonly string CommandDescription = Resources.CommandDescriptionStatus;

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
                ConsoleExtension.WriteStatus($"* {Resources.LabelProfile}: {status.ProfileName}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resources.LabelAccountValidFor}: {status.TimeLeft}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resources.LabelHelp}: {Resources.HelpUrl}");
                ConsoleExtension.WriteStatus($"* {Resources.LabelVersion}: {status.Version}");
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            });

            return command;
        }
    }
}