using EduRoam.CLI.Commands;

using System.CommandLine;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly List<ICommand> commandsList = new()
        {
            { new Configure() },
            { new Commands.Connect() },
            { new List() },
            { new Refresh() },
            { new Remove() },
            { new Show() },
            { new Uninstall() },

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
