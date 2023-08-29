using Microsoft.Extensions.Configuration;

namespace EduRoam.Connect
{
    internal class Configuration
    {
        private static readonly IConfiguration Config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();

        internal static Uri GeoApiUrl => new(Config.GetSection("App")["GeoApiUrl"] ?? throw new ArgumentNullException(nameof(GeoApiUrl)));

        internal static Uri ProviderApiUrl => new(Config.GetSection("App")["ProviderApiUrl"] ?? throw new ArgumentNullException(nameof(ProviderApiUrl)));
    }
}
