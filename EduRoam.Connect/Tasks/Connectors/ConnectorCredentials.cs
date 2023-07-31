namespace EduRoam.Connect.Tasks.Connectors
{
    public class ConnectorCredentials
    {
        public ConnectorCredentials(string password)
        {
            this.Password = password;
        }

        public ConnectorCredentials(string? userName, string password) : this(password)
        {
            this.UserName = userName;

        }

        public string? UserName { get; }

        public string Password { get; }
    }
}
