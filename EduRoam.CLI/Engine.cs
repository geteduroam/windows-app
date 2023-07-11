using EduRoam.CLI.Commands;

using System.CommandLine;
using System.IO;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly List<ICommand> CommandsList = new ()
        {
            { new ListInstitutions() },
            { new ConnectInstitution() },
            { new Clear() }
        };

        private readonly RootCommand rootCommand;

        public Engine()
        {
            this.rootCommand = new RootCommand("Edu Roam CLI");

            foreach (var command in this.CommandsList)
            {
                this.rootCommand.AddCommand(command.Get());
            }
        }

        public async Task Run(string[] args)
        {
            if (args.Length < 1)
            {
                if (args[0].Equals("x", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            await this.rootCommand.InvokeAsync(args);
            //try
            //{
            //    var command = this.CommandsList[args[0]];

            //    if (command != null)
            //    {
            //        var commandArgs = args.Skip(1).ToArray();
            //        await command.Run(commandArgs);
            //    }
            //}
            //catch (KeyNotFoundException knfexc) {
            //    ConsoleExtension.WriteError($"Unknown command '{args[0]}'");
            //}

        }

        //private string[] GetCommandLine()
        //{
        //    Console.WriteLine("Provide command line command:");
        //    foreach (var commandItem in this.CommandsList)
        //    {
        //        Console.WriteLine($"> {commandItem.Key}");
        //    }
        //    Console.WriteLine("Or type 'x' to exit the console");
        //    Console.WriteLine();
        //    Console.Write("> ");
        //    var commandLine = Console.ReadLine();

        //    if (string.IsNullOrWhiteSpace(commandLine))
        //    {
        //        this.GetCommandLine();
        //    }
        //    return commandLine.Split(" ");
        //}
    }
}
