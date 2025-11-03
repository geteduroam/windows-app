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
        /// <param name="substring">Query to filter institutes</param>
        /// <remarks>
        /// If no providers available try to download them
        /// </remarks>
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        public static async Task<IEnumerable<IdentityProvider>> SearchAsync(string? substring)
        {
            if (string.IsNullOrWhiteSpace(substring))
            {
                return Enumerable.Empty<IdentityProvider>();
            }

            try
            {
                await IdentityProviderDownloader.Instance.LoadProviders();
            }
            catch (Exception)
            {
            }

            return IdentityProviderDownloader.Instance.Loaded
                ? IdentityProviderParser.SortByQuery(
                    IdentityProviderDownloader.Instance.ProvidersSortedByCountry,
                    substring)
                : Enumerable.Empty<IdentityProvider>();
        }

        /// <summary>
        /// Get a Identity Provider from a URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<IdentityProvider?> GetProfileFromUrlAsync(string url)
        {
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                url = $"https://{url}";
            }

            // validate if url is valid
            if(!Uri.IsWellFormedUriString(url.Trim(), UriKind.Absolute))
            {
                throw new EduroamAppUserException(string.Empty, Resources.ErrorOccurredWhileRetreivingProfile);
            }

            using var idpDownloader = IdentityProviderDownloader.Instance;
            var profile = await idpDownloader.DownloadProfileFromUrl(url.Trim());

            idpDownloader.AddHttpProfile(profile);

            return profile;
        }
    }
}