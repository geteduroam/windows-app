using App.Library.ViewModels;

using EduRoam.Connect;

using Microsoft.Extensions.DependencyInjection;

using NLog.Extensions.Logging;

namespace App.Library
{
    public static class ServicesConfiguration
    {
        public static ServiceProvider ConfigureServices() { 
            var configuration = ApplicationConfiguration.InitConfiguration();
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddLogging(builder => builder.AddNLog(configuration));

            return services.BuildServiceProvider();
        }
    }
}
