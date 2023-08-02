using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;

namespace EduRoam.Connect.Tasks
{
    public class GetInstitutesTask
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
        public async Task<IEnumerable<IdentityProvider>> GetAsync(string? query)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders(useGeodata: true);

            if (idpDownloader.Loaded)
            {
                var providers = idpDownloader.ClosestProviders;
                if (string.IsNullOrWhiteSpace(query))
                {
                    return providers;
                }
                return IdentityProviderParser.SortByQuery(providers, query);
            }

            return Enumerable.Empty<IdentityProvider>();
        }
    }
}