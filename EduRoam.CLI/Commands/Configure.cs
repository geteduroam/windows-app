using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Configure : ICommand
    {
        public static readonly string CommandName = "configure";

        public static readonly string CommandDescription = Resource.CommandDescriptionConfigure;

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);
            var profileOption = Options.GetProfileOption(optional: true);
            var eapConfigFileOption = Options.GetEapConfigOption();

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption,
                eapConfigFileOption
            };

            command.EnsureProperEapConfigSourceOptionsAreProvided(eapConfigFileOption, instituteOption, profileOption);

            command.SetHandler(async (FileInfo? eapConfigFile, string? institute, string? profileName) =>
            {
                var connectTask = new GetEapConfigTask();

                EapConfig? eapConfig;

                if (eapConfigFile == null)
                {
                    eapConfig = await connectTask.GetEapConfigAsync(institute!, profileName!);
                }
                else
                {
                    eapConfig = await connectTask.GetEapConfigAsync(eapConfigFile);
                }

                if (eapConfig == null)
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                    return;
                }

                var configurationTask = new ResolveConfigurationTask();
                var configurationResolved = configurationTask.ResolveConfiguration(eapConfig, true);

                if (configurationResolved)
                {
                    ConsoleExtension.WriteStatus(Resource.ConfiguredEap);
                }
                else
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapNotConfigured);
                }

            }, eapConfigFileOption, instituteOption, profileOption);

            return command;
        }
    }
}
