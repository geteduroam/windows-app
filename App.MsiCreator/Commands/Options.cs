using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace App.MsiCreator.Commands
{
    public static class Options
    {
        public static Option<string> GetAppOption(bool optional = false)
        {
            return new Option<string>(
                aliases: new string[] { "-a", "--app" },
                parseArgument: NonEmptyString,
                isDefault: true,
                description: "geteduroam or getgovroam");
        }

        public static Option<FileInfo> GetExePath() => new Option<FileInfo>(
                aliases: new string[] { "-e", "--exe" },
                description: ".exe path");



        private static string NonEmptyString(ArgumentResult result)
        {
            if (!result.Tokens.Any())
            {
                result.ErrorMessage = string.Format("{0} required", result.Argument.Name);
                return string.Empty;
            }

            var value = result.Tokens.Single().Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                result.ErrorMessage = string.Format("{0} required", result.Argument.HelpName);
                return string.Empty;

            }
            return value;
        }

    }
}
