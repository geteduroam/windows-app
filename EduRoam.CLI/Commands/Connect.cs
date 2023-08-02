using EduRoam.CLI.Commands.Connections;
using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static readonly string CommandName = "connect";

        public static readonly string CommandDescription = Resource.CommandDescriptionConnect;

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);
            var profileOption = Options.GetProfileOption(optional: true);
            var eapConfigFileOption = Options.GetEapConfigOption();
            var certificatePathOption = Options.GetCertificatePathOption();
            var forceOption = Options.GetForceOption();

            var command = new Command(CommandName, CommandDescription)
            {
                instituteOption,
                profileOption,
                eapConfigFileOption,
                certificatePathOption,
                forceOption
            };

            command.EnsureProperEapConfigSourceOptionsAreProvided(eapConfigFileOption, instituteOption, profileOption);

            command.SetHandler(async (FileInfo? eapConfigFile, string? institute, string? profileName, FileInfo? certificateFile, bool force) =>
            {
                var eapConfig = await GetEapConfigAsync(eapConfigFile, institute, profileName);
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

                OutputCertificatesStatus(eapConfig);
                var success = ConfigureCertificates(eapConfig, force);

                if (!success)
                {
                    ConsoleExtension.WriteError(Resource.ErrorRequiredCertificatesNotInstalled);
                    return;
                }

                var connectTask = new ConnectTask();

                var connector = await connectTask.GetConnectorAsync();
                if (connector == null)
                {
                    ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                    return;
                }

                try
                {
                    IList<string> messages = new List<string>();
                    IConnection connection = connector switch
                    {
                        CredentialsConnector credentialsConnector => new CredentialsConnection(credentialsConnector),
                        CertPassConnector certPassConnector => new CertPassConnection(certPassConnector),
                        CertAndCertPassConnector certAndCertPassConnector => new CertAndCertPassConnection(certAndCertPassConnector, certificateFile),
                        DefaultConnector defaultConnector => new DefaultConnection(defaultConnector),
                        _ => throw new NotSupportedException(string.Format(Resource.ErrorUnsupportedConnectionType, connector.GetType().Name)),
                    };

                    var status = await connection.ConfigureAndConnectAsync(force);
                    if (status.Success)
                    {
                        ConsoleExtension.WriteStatus(string.Join("\n", messages));
                    }
                    else
                    {
                        ConsoleExtension.WriteError(string.Join("\n", messages));
                    }

                }
                catch (EduroamAppUserException ex)
                {
                    // TODO, NICE TO HAVE: log the error
                    ConsoleExtension.WriteError(Resource.ErrorNoConnection, ex.UserFacingMessage);
                }

                catch (ArgumentException exc)
                {
                    ConsoleExtension.WriteError(exc.Message);
                }
                catch (ApiParsingException e)
                {
                    // Must never happen, because if the discovery is reached,
                    // it must be parseable. Logging has been done upstream.
                    ConsoleExtension.WriteError(Resource.ErrorApi);
                    ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
                }
                catch (ApiUnreachableException)
                {
                    ConsoleExtension.WriteError(Resource.ErrorNoInternet);
                }
            }, eapConfigFileOption, instituteOption, profileOption, certificatePathOption, forceOption);

            return command;
        }

        private static Task<EapConfig?> GetEapConfigAsync(FileInfo? eapConfigFile, string? institute, string? profileName)
        {
            var connectTask = new GetEapConfigTask();

            if (eapConfigFile == null)
            {
                return connectTask.GetEapConfigAsync(institute!, profileName!);
            }

            return connectTask.GetEapConfigAsync(eapConfigFile);
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

        private static bool ConfigureCertificates(EapConfig eapConfig, bool force)
        {
            var configurationTask = new ConfigureTask(eapConfig);

            var certificatesResolved = configurationTask.ConfigureCertificates(force);

            if (!certificatesResolved.Success && !force)
            {
                Console.WriteLine(Resource.RequestToInstallCertificates);
                var confirm = Interaction.GetConfirmation();

                if (confirm)
                {
                    certificatesResolved = configurationTask.ConfigureCertificates(true);
                }
            }

            return certificatesResolved.Success;
        }
    }
}
