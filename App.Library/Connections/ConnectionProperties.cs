using System.IO;

namespace App.Library.Connections
{
    public class ConnectionProperties
    {
        public string? UserName { get; set; }

        public string? Password { get; set; }

        public FileInfo? CertificatePath { get; set; }
    }
}
