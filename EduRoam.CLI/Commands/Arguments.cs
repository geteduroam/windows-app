using System.CommandLine;
using System.CommandLine.Parsing;

namespace EduRoam.CLI.Commands
{
    public static class Arguments
    {
        public static Option<string> Institute => new(
                name: "--i",
                parseArgument: NonEmptyString,
                isDefault: true,
                description: "The name of the institute to connect to.");

        public static Option<string> Profile => new(
                name: "--p",
                parseArgument: NonEmptyString,
                isDefault: true,
                description: "Institute's profile to connect to.");

        public static Option<bool> Force => new(
                name: "--f",
                description: "Force automatic configuration if the profile is not already configured (fully).",
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
