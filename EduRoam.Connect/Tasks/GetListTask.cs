using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class GetListTask
    {
        /// <summary>
        /// Get a list of Identity Providers.
        /// </summary>
        /// <remarks>
        /// If no providers available try to download them
        /// </remarks>
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        public async Task<IEnumerable<IdentityProvider>> GetAsync()
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders(useGeodata: true);
            if (idpDownloader.Loaded)
            {
                return idpDownloader.ClosestProviders;
            }

            return Enumerable.Empty<IdentityProvider>();
        }
    }
}