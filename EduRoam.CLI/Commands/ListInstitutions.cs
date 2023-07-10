using EduRoam.Connect;
using EduRoam.Connect.Exceptions;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    public class ListInstitutions : ICommand
    {
        public static string CommandName => "list-institutions";

        public static string CommandDescription => "list-institutions";

        public Command Get()
        {
            var command = new Command(CommandName, CommandDescription)
            {
                
            };
            
            command.SetHandler(async () =>
            {
                await LoadProviders();
            });

            return command;
        }

        public async Task Run(string[] args)
        {
            Console.WriteLine($"Run {CommandName} with the following args: {string.Join(", ", args)}");

            await this.LoadProviders();
        }

        /// <summary>
		/// If no providers available try to download them
		/// </summary>
		private async Task LoadProviders()
        {
            using var idpDownloader = new IdentityProviderDownloader();

            try
            {
                await idpDownloader.LoadProviders(useGeodata: true);
                if (idpDownloader.Loaded)
                {
                    var closestProviders = idpDownloader.ClosestProviders;
                    foreach (var provider in closestProviders)
                    {
                        Console.WriteLine(provider.Name);
                    }
                }
            }
            catch (ApiParsingException e)
            {
                // Must never happen, because if the discovery is reached,
                // it must be parseable. Logging has been done upstream.
                ConsoleExtension.WriteError("API error");
                ConsoleExtension.WriteError(e.Message, e.GetType().ToString());                
            }
            catch (ApiUnreachableException)
            {
                ConsoleExtension.WriteError("No internet connection");
            }
            

        }
    }
}
