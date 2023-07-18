using EduRoam.Connect;
using EduRoam.Connect.Install;
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
                Console.WriteLine(Resource.UninstallWarning);

                if (CertificateStore.AnyRootCaInstalledByUs())
                {
                    Console.WriteLine(Resource.UninstallCertificatesWarning);
                    Console.WriteLine();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine();
                }

                var confirmed = Confirm.GetConfirmation();

                if (confirmed)
                {
                    var task = new UninstallTask();
                    task.Uninstall();
                }
                else
                {
                    Console.WriteLine(Resource.ErrorNotUninstalled);
                }
            });

            return command;
        }

    }
}
