using EduRoam.CLI.Commands;

using System.CommandLine;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly List<ICommand> CommandsList = new()
        {
            { new ListInstitutions() },
            { new ListProfiles() },
            { new Commands.ConnectByProfile() },
            { new Clear() }
        };

        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand("Edu Roam CLI");

            foreach (var command in this.CommandsList)
            {
                this.rootCommand.AddCommand(command.GetCommand());
            }
        }

        public async Task Run(string[] args)
        {
            await this.rootCommand.InvokeAsync(args);
        }

    }
}
