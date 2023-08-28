﻿using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    internal class Remove : ICommand
    {
        public static readonly string CommandName = "remove";

        public static readonly string CommandDescription = Resources.CommandDescriptionRemove;

        public Command GetCommand()
        {
            var command = new Command(CommandName, CommandDescription);

            command.SetHandler(() =>
            {
                var profilesTask = new ProfilesTask();

                Console.Write(Resources.ProfileRemoveConfirmation, profilesTask.GetCurrentProfileName());
                var confirmed = Interaction.GetConfirmation();

                if (confirmed)
                {
                    RemoveWiFiConfigurationTask.Remove(omitRootCa: true);
                }
            });

            return command;
        }
    }
}
