using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Install : ICommand
    {
        public static readonly string CommandName = "install";

        public static readonly string CommandDescription = Resource.CommandDescriptionInstall;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var installTask = new InstallTask();
                installTask.Install();

                Console.WriteLine(Resource.Done);
            });

            return command;
        }
    }
}
