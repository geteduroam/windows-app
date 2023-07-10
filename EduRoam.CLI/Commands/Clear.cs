using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    internal class Clear : ICommand
    {
        public readonly static string CommandName = "clear" ;

        public void Run(string[] args)
        {
            Console.Clear();

        }
    }
}
