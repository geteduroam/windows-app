using EduRoam.Connect;
using EduRoam.Connect.Tasks;

using System.CommandLine;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    internal class Status : ICommand
    {
        public static readonly string CommandName = "status";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionStatus;

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
                ConsoleExtension.WriteStatus($"* {SharedResources.LabelProfile}: {status.ProfileName}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {SharedResources.LabelAccountValidFor}: {status.TimeLeft}");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {SharedResources.LabelHelp}: {Resources.HelpUrl}");
                ConsoleExtension.WriteStatus($"* {SharedResources.LabelVersion}: {status.Version}");
                ConsoleExtension.WriteStatus("***********************************************");
                Console.WriteLine();
            });

            return command;
        }
    }
}