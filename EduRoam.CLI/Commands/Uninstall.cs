using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Uninstall : ICommand
    {
        public readonly static string CommandName = "uninstall";

        public static string CommandDescription => "Uninstall application";

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription)
            {

            };

            command.SetHandler(() =>
            {
                Console.WriteLine(
                "You are currently in the process of completly uninstalling geteduroam.\n" +
                (CertificateStore.AnyRootCaInstalledByUs()
                    ? "This means uninstalling all the trusted root certificates installed by this application.\n\n"
                    : "\n") +
                "Are you sure you want to continue? (y/N)");

                var choice = Console.ReadKey();
                if (choice.KeyChar == 'y' || choice.KeyChar == 'Y')
                {
                    var task = new UninstallTask();
                    task.Uninstall();
                }
                else
                {
                    Console.WriteLine("geteduroam has not been uninstalled.");
                }
            });

            return command;
        }

    }
}
