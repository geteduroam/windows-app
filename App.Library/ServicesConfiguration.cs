using App.Library.ViewModels;
using Microsoft.Extensions.DependencyInjection;

using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace App.Library
{
    public static class ServicesConfiguration
    {
        public static ServiceProvider ConfigureServices() { 
            var configuration = getNLogConfiguration();
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddLogging(builder => builder.AddNLog(configuration));

            return services.BuildServiceProvider();
        }
        private static LoggingConfiguration getNLogConfiguration()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"${{specialfolder:folder=LocalApplicationData}}/{Settings.Settings.ApplicationName}/{Settings.Settings.ApplicationName}-${{shortdate}}.log" };
#if DEBUG
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
#endif

            // Rules for mapping loggers to targets            
#if DEBUG
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
#endif
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, logfile);

            return config;
        }
    }
}
