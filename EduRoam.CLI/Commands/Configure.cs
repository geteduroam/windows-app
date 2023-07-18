using EduRoam.Connect;
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
            var instituteOption = Options.GetInstituteOption();
            var profileOption = Options.GetProfileOption();

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
                    ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                    return;
                }

                var connectTask = new ResolveConfigurationTask();


                var configurationResolved = connectTask.ResolveConfiguration(eapConfig, true);

                if (configurationResolved)
                {
                    ConsoleExtension.WriteStatus(Resource.ConfiguredEap);
                }
                else
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapNotConfigured);
                }

            }, instituteOption, profileOption);

            return command;
        }
    }
}
