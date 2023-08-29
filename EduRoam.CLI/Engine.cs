using EduRoam.CLI.Commands;

using System.CommandLine;
using System.Reflection;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand(SharedResources.AppTitle);
            var commandsList = GetCommandList();

            foreach (var command in commandsList)
            {
                this.rootCommand.AddCommand(command.GetCommand());
            }
        }

        public async Task Run(string[] args)
        {
            await this.rootCommand.InvokeAsync(args);
        }

        public static List<ICommand> GetCommandList()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var commandClasses = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            var commands = new List<ICommand>();

            foreach (var commandClass in commandClasses)
            {
                if (Activator.CreateInstance(commandClass) is not ICommand command)
                {
                    continue;
                }

                commands.Add(command);
            }

            return commands;
        }
    }
}
