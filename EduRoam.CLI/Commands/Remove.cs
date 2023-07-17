using EduRoam.Connect.Store;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public static readonly string CommandName = "remove";

        public static readonly string CommandDescription = "Remove configured Wi-Fi profile and/or root certificates";

        private readonly BaseConfigStore store = new RegistryStore();

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var profileName = this.store.IdentityProvider?.DisplayName ?? "geteduroam";
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
