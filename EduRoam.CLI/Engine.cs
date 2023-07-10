using EduRoam.CLI.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly Dictionary<string, ICommand> CommandsList = new Dictionary<string, ICommand>()
        {
            { Commands.Connect.CommandName, new Commands.Connect() },
            { Commands.Clear.CommandName, new Commands.Clear() }
        };

        public void Run(string[] args)
        {
            if (args.Length < 1)
            {
                args = GetCommandLine();

                if (args[0].Equals("x", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            var command = this.CommandsList[args[0]];

            if (command != null)
            {
                var commandArgs = args.Skip(1).ToArray();
                command.Run(commandArgs);
            }


        }

        private string[] GetCommandLine()
        {
            Console.WriteLine("Provide command line command:");
            foreach (var commandItem in this.CommandsList)
            {
                Console.WriteLine($"> {commandItem.Key}");
            }
            Console.WriteLine("Or type 'x' to exit the console");
            Console.WriteLine();
            var commandLine = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(commandLine))
            {
                this.GetCommandLine();
            }
            return commandLine!.Trim().Split(" ");
        }
    }
}
