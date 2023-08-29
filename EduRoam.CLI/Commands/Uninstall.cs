using EduRoam.Connect;
using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;

using System.CommandLine;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    internal class Uninstall : ICommand
    {
        public static readonly string CommandName = "uninstall";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionUninstall;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                ConsoleExtension.WriteWarning(SharedResources.WarningUninstall);

                if (CertificateStore.AnyRootCaInstalledByUs())
                {
                    ConsoleExtension.WriteWarning(SharedResources.WarningUninstallCertificates);
                    Console.WriteLine();
                }
                Console.WriteLine();

                var confirmed = Interaction.GetConfirmation();

                if (confirmed)
                {
                    UninstallTask.Uninstall((success) => Console.WriteLine("Ready"));
                }
                else
                {
                    ConsoleExtension.WriteError(SharedResources.ErrorNotUninstalled);
                }
            });

            return command;
        }
    }
}
