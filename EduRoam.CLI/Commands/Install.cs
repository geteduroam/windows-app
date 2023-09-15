using EduRoam.Connect.Tasks;

using System;
using System.CommandLine;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    internal class Install : ICommand
    {
        public static readonly string CommandName = "install";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionInstall;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                InstallTask.Install();

                Console.WriteLine(SharedResources.Done);
            });

            return command;
        }
    }
}
