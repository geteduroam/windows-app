using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;

namespace EduRoam.Connect.Tasks.Connectors
{
    public class CertAndCertPassConnector : Connector
    {
        public CertAndCertPassConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.CertAndCertPass;

        public ConnectorCredentials? Credentials { get; set; }
        public FileInfo? CertificatePath { get; set; }

        public (bool, IList<string>) ValidateCertificateAndCredentials()
        {
            var isValid = true;
            var messages = new List<string>();

            if (this.Credentials == null || this.Credentials.Password.Length == 0)
            {
                isValid = false;
                messages.Add(Resource.ErrorInvalidCredentials);
            }

            if (this.CertificatePath == null || !this.CertificatePath.Exists)
            {
                isValid = false;
                messages.Add(Resource.ErrorInvalidCertificatePath);
            }

            return (isValid, messages);
        }

        public override async Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var (configured, messages) = this.ValidateCertificateAndCredentials();

            if (!configured)
            {
                return (configured, messages);
            }

            (configured, messages) = await base.ConfigureAsync(forceConfiguration);

            if (!configured)
            {
                return (configured, messages);
            }

            var eapConfigWithPassphrase = this.eapConfig.WithClientCertificate(this.CertificatePath!.FullName, this.Credentials!.Password.ToString()!);

            if (eapConfigWithPassphrase != null)
            {
                var exception = this.InstallEapConfig(eapConfigWithPassphrase);

                if (exception != null)
                {
                    configured = false;
                    messages = exception.Message.AsListItem();
                }
            }

            return (configured, messages);
        }
    }
}
