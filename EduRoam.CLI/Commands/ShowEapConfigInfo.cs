using EduRoam.Connect;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public class ShowEapConfigInfo
    {
        public static string CommandName => "show-eap-config";

        public static string CommandDescription => "Show EAP Config information";

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
                var connectTask = new GetEapConfigTask();

                try
                {
                    var eapConfig = await connectTask.GetEapConfigAsync(institute, profileName);

                    if (eapConfig == null || !eapConfig.HasInfo)
                    {
                        Console.WriteLine("No EAP Config info to show");
                    }

                    else
                    {

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
                    ConsoleExtension.WriteError("API error");
                    ConsoleExtension.WriteError(e.Message, e.GetType().ToString());
                }
                catch (ApiUnreachableException)
                {
                    ConsoleExtension.WriteError("No internet connection");
                }

            }, instituteOption, profileOption);

            return command;
        }

        private void ShowProfileOverview(EapConfig eapConfig)
        {
            var institutionInfo = eapConfig!.InstitutionInfo;
            var supported = CheckIfEapConfigIsSupported(eapConfig);

            Console.WriteLine();
            ConsoleExtension.WriteStatus("***********************************************");
            ConsoleExtension.WriteStatus($"* {institutionInfo.DisplayName}");
            ConsoleExtension.WriteStatus($"* {institutionInfo.Description}");
            if (!HasContactInfo(eapConfig.InstitutionInfo))
            {
                ConsoleExtension.WriteStatusIf(institutionInfo.WebAddress != null, $"* {institutionInfo.WebAddress}");
                ConsoleExtension.WriteStatusIf(institutionInfo.EmailAddress != null, $"* {institutionInfo.EmailAddress}");
                ConsoleExtension.WriteStatusIf(institutionInfo.Phone != null, $"* {institutionInfo.Phone}");
            }
            ConsoleExtension.WriteStatus($"* supported: {(supported ? "✓" : "x")}");
            ConsoleExtension.WriteStatus("***********************************************");
            Console.WriteLine();

        }

        private static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
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

        private static bool HasContactInfo(EapConfig.ProviderInfo info)
        {
            var hasWebAddress = !string.IsNullOrEmpty(info.WebAddress);
            var hasEmailAddress = !string.IsNullOrEmpty(info.EmailAddress);
            var hasPhone = !string.IsNullOrEmpty(info.Phone);
            return (hasWebAddress || hasEmailAddress || hasPhone);
        }
    }
}
