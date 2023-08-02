using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Install : ICommand
    {
        public static readonly string CommandName = "install";

        public static readonly string CommandDescription = Resources.CommandDescriptionInstall;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var installTask = new InstallTask();
                installTask.Install();

                Console.WriteLine(Resources.Done);
            });

            return command;
        }
    }
}
