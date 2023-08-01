namespace EduRoam.CLI.Commands.Connections
{
    internal interface IConnection
    {
        Task<(bool connected, IList<string> messages)> ConfigureAndConnectAsync(bool force);
    }
}
