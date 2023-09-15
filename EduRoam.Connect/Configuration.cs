using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EduRoam.Connect
{
    internal class Configuration
    {
        private readonly IConfiguration config;

        internal Configuration()
        {
            var config = new ConfigurationBuilder();

            var assembly = Assembly.GetEntryAssembly()!;
            var appSettings = assembly.GetManifestResourceNames().FirstOrDefault(resource => resource.EndsWith("appsettings.json")) ?? throw new ArgumentException("Missing appsettings.json");

            using var resourceStream = Assembly.GetEntryAssembly()!.GetManifestResourceStream(appSettings);
            Debug.Assert(resourceStream != null);

            config.AddJsonStream(resourceStream);
            this.config = config.Build();
        }

        internal Uri ProviderApiUrl => new(this.config.GetSection("App")["ProviderApiUrl"] ?? throw new ArgumentNullException(nameof(this.ProviderApiUrl)));

        internal string AppRegistryNamespace => this.config.GetSection("App")["AppRegistryNamespace"] ?? throw new ArgumentNullException(nameof(this.AppRegistryNamespace));
    }
}
