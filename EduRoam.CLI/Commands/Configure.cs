using EduRoam.Connect;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Configure : ICommand
    {
        public static string CommandName => "configure";

        public static string CommandDescription => "Configure Wi-Fi based on a institution profile";

        public Command GetCommand()
        {
            var instituteOption = Arguments.Institute;
            var profileOption = Arguments.Profile;

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption
            };


            command.SetHandler(async (string institute, string profileName) =>
            {
                var getEapConfig = new GetEapConfigTask();
                var eapConfig = await getEapConfig.GetEapConfigAsync(institute, profileName);

                if (eapConfig == null)
                {
                    ConsoleExtension.WriteError($"Could not connect, EAP Configuration is empty");
                    return;
                }

                var connectTask = new ResolveConfigurationTask();


                var configurationResolved = connectTask.ResolveConfiguration(eapConfig, true);

                if (configurationResolved)
                {
                    ConsoleExtension.WriteStatus("EAP is configured");
                }
                else
                {
                    ConsoleExtension.WriteStatus("EAP could not be configured");
                }

            }, instituteOption, profileOption);

            return command;
        }
    }
}
