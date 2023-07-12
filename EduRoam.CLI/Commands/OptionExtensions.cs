using System.CommandLine.Parsing;

namespace EduRoam.CLI.Commands
{
    internal static class OptionExtensions
    {
        internal static string NonEmptyString(ArgumentResult result)
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
