using System.Security;

namespace EduRoam.Connect.Tasks.Connectors
{
    public class ConnectorCredentials
    {
        public ConnectorCredentials(SecureString password)
        {
            this.Password = password;
        }

        public ConnectorCredentials(string? userName, SecureString password) : this(password)
        {
            this.UserName = userName;

        }

        public string? UserName { get; }

        public SecureString Password { get; }
    }
}
