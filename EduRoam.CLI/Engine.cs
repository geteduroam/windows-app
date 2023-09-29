using EduRoam.CLI.Commands;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand(ApplicationResources.GetString("AppTitle"));
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
