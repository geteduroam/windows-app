using EduRoam.Connect;
using EduRoam.Connect.Install;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Uninstall : ICommand
    {
        public static readonly string CommandName = "uninstall";

        public static readonly string CommandDescription = Resource.CommandDescriptionUninstall;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                ConsoleExtension.WriteWarning(Resource.WarningUninstall);

                if (CertificateStore.AnyRootCaInstalledByUs())
                {
                    ConsoleExtension.WriteWarning(Resource.WarningUninstallCertificates);
                    Console.WriteLine();
                }
                Console.WriteLine();

                var confirmed = Interaction.GetConfirmation();

                if (confirmed)
                {
                    var task = new UninstallTask();
                    task.Uninstall((success) => Console.WriteLine("Ready"));
                }
                else
                {
                    ConsoleExtension.WriteError(Resource.ErrorNotUninstalled);
                }
            });

            return command;
        }
    }
}
