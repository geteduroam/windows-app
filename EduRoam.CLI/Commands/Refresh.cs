using EduRoam.Connect.Tasks;

using System;
using System.CommandLine;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI.Commands
{
    internal class Refresh : ICommand
    {
        public static readonly string CommandName = "refresh";

        public static readonly string CommandDescription = SharedResources.CommandDescriptionRefresh;

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
