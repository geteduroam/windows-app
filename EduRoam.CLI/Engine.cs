using EduRoam.CLI.Commands;
using EduRoam.Connect;

using System.CommandLine;
using System.Reflection;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand(Resource.AppTitle);
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
                var command = Activator.CreateInstance(commandClass) as ICommand;

                if (command == null) { continue; }

                commands.Add(command);
            }

            return commands;
        }
    }
}
