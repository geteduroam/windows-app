using EduRoam.CLI.Commands;

using System.CommandLine;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly List<ICommand> commandsList = new()
        {
            { new Clear() },
            { new Configure() },
            { new ConnectByProfile() },
            { new ListInstitutions() },
            { new ListProfiles() },
            { new ShowEapConfigInfo() },
        };

        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand("Edu Roam CLI");

            foreach (var command in this.commandsList)
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
