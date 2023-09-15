using EduRoam.Connect;

using Microsoft.Extensions.DependencyInjection;

using NLog.Extensions.Logging;

namespace EduRoam.CLI
{
    public static class ServicesConfiguration
    {
        public static ServiceProvider ConfigureServices()
        {
            var configuration = ApplicationConfiguration.InitConfiguration();
            var services = new ServiceCollection();

            services.AddSingleton<Engine>();
            services.AddLogging(builder => builder.AddNLog(configuration));

            return services.BuildServiceProvider();
        }
    }
}
