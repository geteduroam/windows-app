using EduRoam.Localization;

using System.CommandLine;
using System.CommandLine.Parsing;

namespace EduRoam.CLI
{
    public static class Options
    {
        public static Option<string> GetInstituteOption(bool optional = false)
        {
            if (optional)
            {
                return new(aliases: new string[] { "-i", "--institute" }, description: Resources.OptionDescriptionInstitute);
            }

            return new(
                aliases: new string[] { "-i", "--institute" },
                parseArgument: NonEmptyString,
                isDefault: true,
                description: Resources.OptionDescriptionInstitute);
        }

        public static Option<string> GetProfileOption(bool optional = false)
        {
            if (optional)
            {
                return new(aliases: new string[] { "-p", "--profile" }, description: Resources.OptionDescriptionProfile);
            }

            return new(
                aliases: new string[] { "-p", "--profile" },
                parseArgument: NonEmptyString,
                isDefault: true,
                description: Resources.OptionDescriptionProfile);
        }

        public static Option<FileInfo> GetEapConfigOption() => new(
                aliases: new string[] { "-c", "--config" },
                description: Resources.OptionDescriptionEAPConfig);

        public static Option<bool> GetForceOption() => new(
                aliases: new string[] { "-f", "--force" },
                description: Resources.OptionDescriptionForce,
                getDefaultValue: () => false);

        public static Option<string> GetQueryOption() => new(
                aliases: new string[] { "-q", "--query" },
                description: Resources.OptionDescriptionQuery);

        public static Option<FileInfo> GetCertificatePathOption() => new(
                aliases: new string[] { "-cp", "--certificate-path" },
                description: Resources.OptionDescriptionCertificatePath
                );

        /// <summary>
        /// Ensure the user has provided an Eap Config option or
        ///  both Institute and Profile options
        /// </summary>
        /// <param name="instituteOption"></param>
        /// <param name="profileOption"></param>
        /// <param name="eapConfigFileOption"></param>
        /// <param name="command"></param>
        public static void EnsureProperEapConfigSourceOptionsAreProvided(this Command command, Option<FileInfo> eapConfigFileOption, Option<string> instituteOption, Option<string> profileOption)
        {
            command.AddValidator(validator =>
            {
                var instituteOptionValue = validator.GetValueForOption(instituteOption);
                var profileOptionValue = validator.GetValueForOption(profileOption);
                var eapConfigFileArgValue = validator.GetValueForOption(eapConfigFileOption);

                if (eapConfigFileArgValue == null && (string.IsNullOrWhiteSpace(instituteOptionValue) || string.IsNullOrWhiteSpace(profileOptionValue)))
                {
                    validator.ErrorMessage = string.Format(Resources.ErrorShowCommandOptions, string.Join("\\", eapConfigFileOption.Aliases), string.Join("\\", instituteOption.Aliases), string.Join("\\", profileOption.Aliases));
                }
            });
        }

        private static string NonEmptyString(ArgumentResult result)
        {
            if (!result.Tokens.Any())
            {
                result.ErrorMessage = string.Format(Resources.OptionRequired, result.Argument.Name);
                return string.Empty;
            }

            var value = result.Tokens.Single().Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                result.ErrorMessage = string.Format(Resources.ErrorOptionIsEmpty, result.Argument.HelpName);
                return string.Empty;

            }
            return value;
        }

    }
}
