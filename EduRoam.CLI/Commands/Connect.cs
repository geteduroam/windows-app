using EduRoam.Connect;
using EduRoam.Connect.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.CLI.Commands
{
    public class Connect : ICommand
    {
        public static string CommandName => "connect";

        private readonly IdentityProviderDownloader idpDownloader = new IdentityProviderDownloader();

        

        public async Task Run(string[] args)
        {
            Console.WriteLine($"Run connect with the following args: {string.Join(", ", args)}");

            await this.LoadProviders();
        }

        /// <summary>
		/// If no providers available try to download them
		/// </summary>
		private async Task LoadProviders()
        {           
            try
            {
                await this.idpDownloader.LoadProviders(useGeodata: true);
                var loaded = this.idpDownloader.Loaded;
                ConsoleExtension.WriteStatus($"Identity provider loaded? {loaded}");
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
