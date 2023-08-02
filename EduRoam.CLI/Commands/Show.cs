using EduRoam.Connect;
using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class Show : ICommand
    {
        public static readonly string CommandName = "show";

        public static readonly string CommandDescription = Resource.CommandDescriptionShow;

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

                    if (eapConfig == null || !HasInfo(eapConfig))
                    {
                        Console.WriteLine(Resource.NoEAPConfig);
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
                    ConsoleExtension.WriteError(Resource.ErrorApi);
                    ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
                }
                catch (ApiUnreachableException)
                {
                    ConsoleExtension.WriteError(Resource.ErrorNoInternet);
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
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.WebAddress), $"* {Resource.LabelWeb}: {institutionInfo.WebAddress}");
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.EmailAddress), $"* {Resource.LabelEmail}: {institutionInfo.EmailAddress}");
                ConsoleExtension.WriteStatusIf(!string.IsNullOrEmpty(institutionInfo.Phone), $"* {Resource.LabelPhone}: {institutionInfo.Phone}");
            }

            ConsoleExtension.WriteStatus("* ");
            ConsoleExtension.WriteStatus($"* {Resource.LabelSupported}: {Interaction.GetYesNoText(supported)}");

            if (!string.IsNullOrWhiteSpace(institutionInfo.TermsOfUse))
            {
                ConsoleExtension.WriteStatus("* ");
                ConsoleExtension.WriteStatus($"* {Resource.LabelTermsOfUse}:");
                ConsoleExtension.WriteStatus($"* {institutionInfo.TermsOfUse.Trim()}");
                ConsoleExtension.WriteStatus("* ");
            }

            ConsoleExtension.WriteStatus("***********************************************");
            Console.WriteLine();
        }

        private static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                ConsoleExtension.WriteError(Resource.ErrorUnsupportedProfile);
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
