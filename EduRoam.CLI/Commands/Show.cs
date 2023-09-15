using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System;
using System.CommandLine;
using System.IO;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    public class Show : ICommand
    {
        public static readonly string CommandName = "show";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionShow;

        public Command GetCommand()
        {
            var instituteOption = Options.GetInstituteOption(optional: true);
            var profileOption = Options.GetProfileOption(optional: true);
            var eapConfigFileOption = Options.GetEapConfigOption();

            var command = new Command(CommandName, CommandDescription)
            {
                eapConfigFileOption,
                instituteOption,
                profileOption,
            };

            command.EnsureProperEapConfigSourceOptionsAreProvided(eapConfigFileOption, instituteOption, profileOption);

            command.SetHandler(async (FileInfo? eapConfigFile, string? institute, string? profileName) =>
            {
                try
                {
                    EapConfig? eapConfig;

                    if (eapConfigFile == null)
                    {
                        var eapConfiguration = new EapConfigTask();
                        eapConfig = await eapConfiguration.GetEapConfigAsync(institute!, profileName!);
                    }
                    else
                    {
                        eapConfig = EapConfigTask.GetEapConfig(eapConfigFile);
                    }

                    if (eapConfig == null || !HasInfo(eapConfig))
                    {
                        Console.WriteLine(SharedResources.NoEAPConfig);
                    }
                    else
                    {
                        ShowProfileOverview(eapConfig);
                    }
                }
                catch (Exception exc) when (exc is ArgumentException || exc is UnknownInstituteException || exc is UnknownProfileException)
                {
                    ConsoleExtension.WriteError(exc.Message);
                }
                catch (EduroamAppUserException ex)
                {
                    ConsoleExtension.WriteError(
                        ex.UserFacingMessage);
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

            }, eapConfigFileOption, instituteOption, profileOption);

            return command;
        }

        private static void ShowProfileOverview(EapConfig eapConfig)
        {
            var institutionInfo = eapConfig!.InstitutionInfo;
            var supported = CheckIfEapConfigIsSupported(eapConfig);

            Console.WriteLine();
            ConsoleExtension.WriteStatus("***********************************************");
            ConsoleExtension.WriteStatus($"* {institutionInfo.DisplayName}");
            ConsoleExtension.WriteStatus($"* {institutionInfo.Description}");
            if (!HasContactInfo(eapConfig.InstitutionInfo))
            {
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.WebAddress), $"* {SharedResources.LabelWeb}: {institutionInfo.WebAddress}");
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.EmailAddress), $"* {SharedResources.LabelEmail}: {institutionInfo.EmailAddress}");
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.Phone), $"* {SharedResources.LabelPhone}: {institutionInfo.Phone}");
            }

            ConsoleExtension.WriteStatus("* ");
            ConsoleExtension.WriteStatus($"* {SharedResources.LabelSupported}: {Interaction.GetYesNoText(supported)}");

            if (!string.IsNullOrWhiteSpace(institutionInfo.TermsOfUse))
            {
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {SharedResources.LabelTermsOfUse}:");
                ConsoleExtension.WriteStatus($"* {institutionInfo.TermsOfUse.Trim()}");
                ConsoleExtension.WriteStatus("* ");
            }

            ConsoleExtension.WriteStatus("***********************************************");
            Console.WriteLine();
        }

        private static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EapConfigTask.IsEapConfigSupported(eapConfig))
            {
                ConsoleExtension.WriteError(SharedResources.ErrorUnsupportedProfile);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Used to determine if an eapconfig has enough info
        /// for the ProfileOverview page to show
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static bool HasInfo(EapConfig config)
            => !string.IsNullOrEmpty(config.InstitutionInfo.WebAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.EmailAddress)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Description)
            || !string.IsNullOrEmpty(config.InstitutionInfo.Phone)
            || !string.IsNullOrEmpty(config.InstitutionInfo.TermsOfUse);

        private static bool HasContactInfo(ProviderInfo info)
        {
            var hasWebAddress = !string.IsNullOrEmpty(info.WebAddress);
            var hasEmailAddress = !string.IsNullOrEmpty(info.EmailAddress);
            var hasPhone = !string.IsNullOrEmpty(info.Phone);

            return (hasWebAddress || hasEmailAddress || hasPhone);
        }
    }
}
