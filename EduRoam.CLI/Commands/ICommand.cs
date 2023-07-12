using System.CommandLine;

namespace EduRoam.CLI.Commands
{
    public interface ICommand
    {
        Command GetCommand();
    }
}
