using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.Connections
{
    internal interface IConnection
    {
        Task<TaskStatus> ConfigureAndConnectAsync(ConnectionProperties properties);
    }
}
