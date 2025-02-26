using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduRoam.Connect.Tasks
{
    public class InstitutesTask
    {
        /// <summary>
        /// Get a list of Identity Providers.
        /// </summary>
        /// <param name="query">Query to filter institutes</param>
        /// <remarks>
        /// If no providers available try to download them
        /// </remarks>
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        public static async Task<IEnumerable<IdentityProvider>> GetAsync(string? query)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders();

            if (idpDownloader.Loaded)
            {
                var providers = idpDownloader.ClosestProviders;
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Enumerable.Empty<IdentityProvider>();
                }
                return IdentityProviderParser.SortByQuery(providers, query);
            }

            return Enumerable.Empty<IdentityProvider>();
        }

        /// <summary>
        /// Get a Identity Provider from a URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<IdentityProvider?> GetProfileFromUrlAsync(string url)
        {
            // validate if url is valid
            if(!Uri.IsWellFormedUriString(url.Trim(), UriKind.Absolute))
            {
                throw new EduroamAppUserException(string.Empty, Resources.ErrorOccurredWhileRetreivingProfile);
            }

            using var idpDownloader = new IdentityProviderDownloader();
            var profile = await idpDownloader.DownloadProfileFromUrl(url.Trim());

            await idpDownloader.AddHttpProfile(profile); 

            return profile;
        }
    }
}