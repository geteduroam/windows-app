using Microsoft.Extensions.Configuration;

using System;
using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect
{
    internal class Configuration
    {
        private readonly IConfiguration config;

        internal Configuration()
        {
            this.config = ApplicationConfiguration.InitConfiguration();
        }

        internal Uri ProviderApiUrl => new(this.config.GetSection("App")["ProviderApiUrl"] ?? throw new ArgumentNullException(nameof(this.ProviderApiUrl)));

        internal string AppRegistryNamespace => this.config.GetSection("App")["AppRegistryNamespace"] ?? throw new ArgumentNullException(nameof(this.AppRegistryNamespace));

        internal StoreLocation CertificateStore => (StoreLocation)Enum.Parse(typeof(StoreLocation), this.config.GetSection("App")["CertificateStore"]);
    }
}
