using EduRoam.Connect;
using EduRoam.Connect.Language;
using EduRoam.Connect.Tasks.Connectors;

namespace EduRoam.CLI.Commands.Connections
{
    internal class CertAndCertPassConnection : IConnection
    {
        private readonly CertAndCertPassConnector connector;
        private readonly FileInfo? certificateFile;

        public CertAndCertPassConnection(CertAndCertPassConnector connector, FileInfo? certificateFile)
        {
            this.connector = connector;
            this.certificateFile = certificateFile;
        }

        public async Task<(bool connected, IList<string> messages)> ConfigureAndConnectAsync(bool force)
        {
            if (this.certificateFile == null)
            {
                var certificatePathOption = Options.GetCertificatePathOption();
                return (false, string.Format(Resource.ErrorNoClientCertificateProvided, string.Join(", ", certificatePathOption.Aliases)).AsListItem());
            }

            Console.Write($"{Resource.Passphrase}: ");
            var passphrase = Input.ReadPassword();

            this.connector.Credentials = new ConnectorCredentials(passphrase);
            this.connector.CertificatePath = this.certificateFile;

            var (validCredentials, messages) = this.connector.ValidateCertificateAndCredentials();
            if (!validCredentials)
            {
                return (validCredentials, messages);
            }

            (var configured, messages) = await this.connector.ConfigureAsync(force);

            if (!configured)
            {
                return (configured, messages);
            }

            return await this.connector.ConnectAsync();
        }
    }
}
