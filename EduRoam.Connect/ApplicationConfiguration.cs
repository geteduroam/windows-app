using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EduRoam.Connect
{
    public class ApplicationConfiguration
    {
        public static IConfiguration InitConfiguration(params string[] args)
        {
            var assembly = Assembly.GetEntryAssembly()!;
            var appSettings = assembly.GetManifestResourceNames().FirstOrDefault(resource => resource.EndsWith("appsettings.json")) ?? throw new ArgumentException("Missing appsettings.json");

            using var resourceStream = Assembly.GetEntryAssembly()!.GetManifestResourceStream(appSettings);
            Debug.Assert(resourceStream != null);

            return new ConfigurationBuilder().AddJsonStream(resourceStream)
                                            .AddJsonFile($"appsettings.test.json", true, true)
                                            .Build();
        }
    }
}
