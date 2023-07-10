namespace EduRoam.CLI.Commands
{
    public interface ICommand
    {
        Task Run(string[] args);
    }
}
