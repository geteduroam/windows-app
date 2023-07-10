using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static string CommandName => "connect";

        public void Run(string[] args)
        {
            Console.WriteLine($"Run connect with the following args: {string.Join(", ", args)}");
        }
    }
}
