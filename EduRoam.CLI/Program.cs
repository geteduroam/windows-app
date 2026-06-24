// See https://aka.ms/new-console-template for more information
using App.Library;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            SharedResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

            var serviceProvider = ServicesConfiguration.ConfigureServices();
            var engine = serviceProvider.GetService<Engine>();
            if (engine == null)
            {
                throw new Exception("Engine service is not registered.");
            }
            await engine.Run(args);

#if DEBUG
            Console.Read();
#endif
        }
    }
}
