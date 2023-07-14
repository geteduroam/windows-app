using EduRoam.Connect;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public readonly static string CommandName = "remove";

        public static string CommandDescription => "Remove configured Wi-Fi profile and/or root certificates";

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription)
            {

            };

            command.SetHandler(() =>
            {

                var profileName = PersistingStore.IdentityProvider?.DisplayName ?? "geteduroam";
                Console.WriteLine($"This will remove all configuration for '{profileName}'. Are you sure? (y/N)");

                var choice = Console.ReadKey();
                if (choice.KeyChar == 'y' || choice.KeyChar == 'Y')
                {
                    var task = new RemoveWiFiConfigurationTask();
                    task.Remove();
                }
            });

            return command;
        }

    }
}
