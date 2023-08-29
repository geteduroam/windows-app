using EduRoam.Connect.Tasks;

using System.CommandLine;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public static readonly string CommandName = "remove";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionRemove;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var profilesTask = new ProfilesTask();

                Console.Write(SharedResources.ProfileRemoveConfirmation, profilesTask.GetCurrentProfileName());
                var confirmed = Interaction.GetConfirmation();

                if (confirmed)
                {
                    RemoveWiFiConfigurationTask.Remove(omitRootCa: true);
                }
            });

            return command;
        }
    }
}
