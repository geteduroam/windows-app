using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    internal class Clear : ICommand
    {
        public readonly static string CommandName = "clear" ;

        public static string CommandDescription => "clear";

        public Command Get()
        {
            var command = new Command(CommandName, CommandDescription)
            {

            };

            command.SetHandler(() =>
            {
                Console.Clear();
            });

            return command;
        }

    }
}
