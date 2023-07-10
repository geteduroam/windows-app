using EduRoam.CLI.Commands;

namespace EduRoam.CLI
{
    public class Engine
    {
        private readonly Dictionary<string, ICommand> CommandsList = new Dictionary<string, ICommand>()
        {
            { Commands.Connect.CommandName, new Commands.Connect() },
            { Commands.Clear.CommandName, new Commands.Clear() }
        };

        public async Task Run(string[] args)
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
                await command.Run(commandArgs);
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
