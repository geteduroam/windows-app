using EduRoam.Connect.Language;
using EduRoam.Connect.Store;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public static readonly string CommandName = "remove";

        public static readonly string CommandDescription = Resource.CommandDescriptionRemove;

        private readonly BaseConfigStore store = new RegistryStore();

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var profileName = this.store.IdentityProvider?.DisplayName ?? Resource.DefaultIdentityProvider;

                Console.Write(Resource.ProfileRemoveConfirmation, profileName);
                var confirmed = Confirm.GetConfirmation();

                if (confirmed)
                {
                    var task = new RemoveWiFiConfigurationTask();
                    task.Remove(omitRootCa: true);
                }
            });

            return command;
        }

    }
}
