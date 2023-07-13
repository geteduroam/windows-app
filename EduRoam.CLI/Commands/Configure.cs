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
            var instituteOption = new Option<string>(
                name: "--i",
                parseArgument: OptionExtensions.NonEmptyString,
                isDefault: true,
                description: "The name of the institute to connect to.");

            var profileOption = new Option<string>(
                name: "--p",
                parseArgument: OptionExtensions.NonEmptyString,
                isDefault: true,
                description: "Institute's profile to connect to.");

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
                    ConsoleExtension.WriteError($"Could not connect, EAP Config is empty");
                    return;
                }

                var connectTask = new ResolveConfigurationTask();


                var configurationResolved = connectTask.ResolveConfiguration(eapConfig, true);

                if (configurationResolved)
                {
                    ConsoleExtension.WriteStatus("EAP Config is configured");
                }
                else
                {
                    ConsoleExtension.WriteStatus("EAP Config could not be configured");
                }

            }, instituteOption, profileOption);

            return command;
        }
    }
}
