﻿using EduRoam.CLI.Commands.Connections;
using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static readonly string CommandName = "connect";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionConnect;

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
                    ConsoleExtension.WriteError(SharedResources.ErrorEapConfigIsEmpty);
                    return;
                }

                if (!EapConfigTask.IsEapConfigSupported(eapConfig))
                {
                    ConsoleExtension.WriteError(SharedResources.ErrorUnsupportedProfile);
                    return;
                }

                OutputCertificatesStatus(eapConfig);
                var success = ConfigureCertificates(eapConfig, force);

                if (!success)
                {
                    ConsoleExtension.WriteError(SharedResources.ErrorRequiredCertificatesNotInstalled);
                    return;
                }

                var connector = await ConnectTask.GetConnectorAsync();
                if (connector == null)
                {
                    ConsoleExtension.WriteError(SharedResources.ErrorEapConfigIsEmpty);
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
                        _ => throw new NotSupportedException(string.Format(SharedResources.ErrorUnsupportedConnectionType, connector.GetType().Name)),
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
                    ConsoleExtension.WriteError(SharedResources.ErrorNoConnection, ex.UserFacingMessage);
                }

                catch (ArgumentException exc)
                {
                    ConsoleExtension.WriteError(exc.Message);
                }
                catch (ApiParsingException e)
                {
                    // Must never happen, because if the discovery is reached,
                    // it must be parseable. Logging has been done upstream.
                    ConsoleExtension.WriteError(SharedResources.ErrorApi);
                    ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
                }
                catch (ApiUnreachableException)
                {
                    ConsoleExtension.WriteError(SharedResources.ErrorNoInternet);
                }
            }, eapConfigFileOption, instituteOption, profileOption, certificatePathOption, forceOption);

            return command;
        }

        private static async Task<EapConfig?> GetEapConfigAsync(FileInfo? eapConfigFile, string? institute, string? profileName)
        {
            var eapConfiguration = new EapConfigTask();

            if (eapConfigFile == null)
            {
                return await eapConfiguration.GetEapConfigAsync(institute!, profileName!);
            }

            return EapConfigTask.GetEapConfig(eapConfigFile);
        }

        private static void OutputCertificatesStatus(EapConfig eapConfig)
        {
            ConsoleExtension.WriteStatus(SharedResources.CertificatesInstallationNotification);

            var configureTask = new ConfigureTask(eapConfig);
            var installers = configureTask.GetCertificateInstallers();
            foreach (var installer in installers)
            {
                Console.WriteLine();
                ConsoleExtension.WriteStatus($"* {string.Format(SharedResources.CertificatesInstallationStatus, installer, Interaction.GetYesNoText(installer.IsInstalled))}");
                Console.WriteLine();
            }
        }

        private static bool ConfigureCertificates(EapConfig eapConfig, bool force)
        {
            var configurationTask = new ConfigureTask(eapConfig);

            var certificatesResolved = configurationTask.ConfigureCertificates(force);

            if (!certificatesResolved.Success && !force)
            {
                Console.WriteLine(SharedResources.RequestToInstallCertificates);
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
