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
        private static ServiceProvider serviceProvider;

        public static async Task Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            SharedResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;

            serviceProvider = ServicesConfiguration.ConfigureServices();

            var engine = serviceProvider.GetService<Engine>() ?? new Engine();

            await engine.Run(args);

#if DEBUG
            Console.Read();
#endif
        }
    }
}
