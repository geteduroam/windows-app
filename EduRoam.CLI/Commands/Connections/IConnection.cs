using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace EduRoam.CLI.Commands.Connections
{
    internal interface IConnection
    {
        Task<TaskStatus> ConfigureAndConnectAsync(bool force);
    }
}
