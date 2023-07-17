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
                return new(name: "--i", description: "The name of the institute to connect to.");
            }

            return new(
                name: "--i",
                parseArgument: NonEmptyString,
                isDefault: true,
                description: "The name of the institute to connect to.");
        }

        public static Option<string> GetProfileOption(bool optional = false)
        {
            if (optional)
            {
                return new(name: "--p", description: "Institute's profile to connect to.");
            }

            return new(
                name: "--p",
                parseArgument: NonEmptyString,
                isDefault: true,
                description: "Institute's profile to connect to.");
        }

        public static Option<FileInfo> GetEapConfigOption() => new(
                aliases: new string[] { "--c", "-config" },
                description: "Path to EAP config .eap-config.");

        public static Option<bool> GetForceOption() => new(
                name: "--f",
                description: "Force action.",
                getDefaultValue: () => false);

        private static string NonEmptyString(ArgumentResult result)
        {
            if (!result.Tokens.Any())
            {
                result.ErrorMessage = $"Option --{result.Argument.Name} is required";
                return "";
            }

            var value = result.Tokens.Single().Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                result.ErrorMessage = $"--{result.Argument.Name} option value cannot be empty or whitespace only";
                return "";

            }
            return value;
        }
    }
}
