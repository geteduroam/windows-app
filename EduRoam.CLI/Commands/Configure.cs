using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;

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

                if (success)
                {
                    await ConfigureProfileAsync(eapConfig, certificateFile, force);
                }
                else
                {
                    ConsoleExtension.WriteError(Resource.ErrorRequiredCertificatesNotInstalled);
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

        private static bool ConfigureCertificates(EapConfig eapConfig, bool force)
        {
            var configurationTask = new ConfigureTask(eapConfig);

            var certificatesResolved = configurationTask.ConfigureCertificates(force);

            if (!certificatesResolved && !force)
            {
                Console.WriteLine(Resource.RequestToInstallCertificates);
                var confirm = Interaction.GetConfirmation();

                if (confirm)
                {
                    certificatesResolved = configurationTask.ConfigureCertificates(true);
                }
            }

            return certificatesResolved;
        }

        private static async Task ConfigureProfileAsync(EapConfig eapConfig, FileInfo? certificateFile, bool force)
        {
            var configurationTask = new ConfigureTask(eapConfig);

            var connector = configurationTask.GetConnector();
            if (connector == null)
            {
                ConsoleExtension.WriteError(Resource.ErrorEapConfigIsEmpty);
                return;
            }

            try
            {
                var connected = false;
                IList<string> messages = new List<string>();

                switch (connector)
                {
                    case CredentialsConnector credentialsConnector:
                        (connected, messages) = await ConfigureWithCredentialsAsync(credentialsConnector, force);
                        break;
                    case CertPassConnector certPassConnector:
                        (connected, messages) = await ConfigureWithCertPassAsync(certPassConnector, force);
                        break;
                    case CertAndCertPassConnector certAndCertPassConnector:
                        (connected, messages) = await ConfigureWithCertAndCertPassAsync(certAndCertPassConnector, certificateFile, force);
                        break;
                    case DefaultConnector defaultConnector:
                        (connected, messages) = await ConfigureAsync(defaultConnector, force);
                        break;
                    default:
                        messages.Add(string.Format(Resource.ErrorUnsupportedConnectionType, connector.GetType().Name));
                        break;
                }

                if (connected)
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

        private static Task<(bool connected, IList<string> messages)> ConfigureAsync(DefaultConnector connector, bool force)
        {
            return connector.ConfigureAsync(force);
        }

        private static async Task<(bool connected, IList<string> messages)> ConfigureWithCertAndCertPassAsync(CertAndCertPassConnector connector, FileInfo? certificateFile, bool force)
        {
            if (certificateFile == null)
            {
                var certificatePathOption = Options.GetCertificatePathOption();
                return (false, string.Format(Resource.ErrorNoClientCertificateProvided, string.Join(", ", certificatePathOption.Aliases)).AsListItem());
            }

            //Console.Write($"{Resource.Passphrase}: ");
            //var passphrase = ReadPassword();

            //connector.Credentials = new ConnectorCredentials(passphrase);
            connector.CertificatePath = certificateFile;

            var (configured, messages) = connector.ValidateCertificateAndCredentials();
            if (configured)
            {
                (configured, messages) = await connector.ConfigureAsync(force);
            }

            return (configured, messages);
        }

        private static async Task<(bool connected, IList<string> messages)> ConfigureWithCertPassAsync(CertPassConnector connector, bool force)
        {
            Console.Write($"{Resource.Passphrase}: ");
            var passphrase = ReadPassword();

            connector.Credentials = new ConnectorCredentials(passphrase);

            var (configured, messages) = connector.ValidateCredentials();
            if (configured)
            {
                (configured, messages) = await connector.ConfigureAsync(force);
            }

            return (configured, messages);
        }

        private static async Task<(bool connected, IList<string> messages)> ConfigureWithCredentialsAsync(CredentialsConnector connector, bool force)
        {
            Console.WriteLine(Resource.ConnectionUsernameAndPasswordRequired);
            Console.Write($"{Resource.Username}: ");
            var userName = Console.ReadLine();

            Console.Write($"{Resource.Password}: ");
            var password = ReadPassword();

            connector.Credentials = new ConnectorCredentials(userName, password);

            var (configured, messages) = connector.ValidateCredentials();
            if (configured)
            {
                (configured, messages) = await connector.ConfigureAsync(force);
            }

            return (configured, messages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Based on https://stackoverflow.com/a/3404522</remarks>
        private static string ReadPassword()
        {
            var pass = string.Empty;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass.Remove(pass.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (keyInfo.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return pass;
        }
    }
}
