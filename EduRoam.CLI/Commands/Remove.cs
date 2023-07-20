using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public static readonly string CommandName = "remove";

        public static readonly string CommandDescription = Resource.CommandDescriptionRemove;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var profilesTask = new GetProfilesTask();

                Console.Write(Resource.ProfileRemoveConfirmation, profilesTask.GetCurrentProfileName());
                var confirmed = Interaction.GetConfirmation();

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
