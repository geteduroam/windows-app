using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Refresh : ICommand
    {
        public static readonly string CommandName = "refresh";

        public static readonly string CommandDescription = Resources.CommandDescriptionRefresh;

        public Command GetCommand()
        {
            var forceOption = Options.GetForceOption();
            forceOption.AddAlias("refresh-force");

            var command = new Command(CommandName, CommandDescription)
            {
                forceOption
            };

            command.SetHandler(async (bool force) =>
            {
                var message = await RefreshTask.RefreshAsync(force);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine(message);
                }
            }, forceOption);

            return command;
        }

    }
}
