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
            var forceOption = Options.GetForceOption();

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption,
                eapConfigFileOption,
                forceOption
            };

            command.EnsureProperEapConfigSourceOptionsAreProvided(eapConfigFileOption, instituteOption, profileOption);

            command.SetHandler(async (FileInfo? eapConfigFile, string? institute, string? profileName, bool force) =>
            {
                var eapConfig = await GetEapConfig(eapConfigFile, institute, profileName);

                if (eapConfig == null)
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                    return;
                }

                if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
                {
                    ConsoleExtension.WriteError(Resource.ErrorUnsupportedProfile);
                    return;
                }

                var success = ConfigureCertificates(eapConfig, force);

                if (success)
                {
                    ConfigureProfile(eapConfig, force);
                }
                else
                {
                    ConsoleExtension.WriteError(Resource.ErrorRequiredCertificatesNotInstalled);
                }

            }, eapConfigFileOption, instituteOption, profileOption, forceOption);

            return command;
        }

        private static Task<EapConfig?> GetEapConfig(FileInfo? eapConfigFile, string? institute, string? profileName)
        {
            var connectTask = new GetEapConfigTask();

            if (eapConfigFile == null)
            {
                return connectTask.GetEapConfigAsync(institute!, profileName!);
            }

            return connectTask.GetEapConfigAsync(eapConfigFile);
        }

        private static bool ConfigureCertificates(EapConfig eapConfig, bool force)
        {
            OutputCertificatesStatus(eapConfig);

            var configurationTask = new ResolveConfigurationTask(eapConfig);

            var certificatesResolved = configurationTask.ResolveCertificates(force);

            if (!certificatesResolved && !force)
            {
                Console.WriteLine(Resource.RequestToInstallCertificates);
                var confirm = Interaction.GetConfirmation();

                if (confirm)
                {
                    certificatesResolved = configurationTask.ResolveCertificates(true);
                }
            }

            return certificatesResolved;
        }

        private static void ConfigureProfile(EapConfig eapConfig, bool force)
        {
            var configurationTask = new ResolveConfigurationTask(eapConfig);
            var configurationResolved = configurationTask.ResolveConfiguration(force);

            if (configurationResolved)
            {
                ConsoleExtension.WriteStatus(Resource.ConfiguredEap);
            }
            else
            {
                ConsoleExtension.WriteError(Resource.ErrorEapNotConfigured);
            }
        }

        private static void OutputCertificatesStatus(EapConfig eapConfig)
        {
            ConsoleExtension.WriteStatus(Resource.CertificatesInstallationNotification);
            var installers = ConnectToEduroam.EnumerateCAInstallers(eapConfig).ToList();
            foreach (var installer in installers)
            {
                Console.WriteLine();
                ConsoleExtension.WriteStatus($"* {string.Format(Resource.CertificatesInstallationStatus, installer, Interaction.GetYesNoText(installer.IsInstalled))}");
                Console.WriteLine();
            }
        }
    }
}
