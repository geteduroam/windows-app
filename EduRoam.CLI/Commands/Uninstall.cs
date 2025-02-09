using App.Settings;

using EduRoam.Connect;
using EduRoam.Connect.Install;
using App.Library.Tasks;

using System;
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
                ConsoleExtension.WriteWarning(string.Format(SharedResources.WarningUninstall, Settings.ApplicationIdentifier));

                if (CertificateStore.AnyRootCaInstalledByUs())
                {
                    ConsoleExtension.WriteWarning(SharedResources.WarningUninstallCertificates);
                    Console.WriteLine();
                }
                Console.WriteLine();

                var confirmed = Interaction.GetConfirmation();

                if (confirmed)
                {
                    if (UninstallTask.Uninstall(false))
                        Console.WriteLine("Ready");
                }
                else
                {
                    ConsoleExtension.WriteError(string.Format(SharedResources.ErrorNotUninstalled, Settings.ApplicationIdentifier));
                }
            });

            return command;
        }
    }
}
