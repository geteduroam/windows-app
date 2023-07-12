using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class GetProfilesTask
    {
        /// <summary>
        /// Get a list of available institute profiles
        /// </summary>
        /// <param name="institute"></param>
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        public async Task<IEnumerable<IdentityProviderProfile>> GetProfilesAsync(string institute)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders(useGeodata: true);
            if (idpDownloader.Loaded)
            {
                var provider = idpDownloader.Providers.SingleOrDefault(provider => provider.Name.Equals(institute, StringComparison.InvariantCultureIgnoreCase));

                if (provider == null)
                {
                    throw new UnknownInstituteException(institute);
                }
                return idpDownloader.GetIdentityProviderProfiles(provider.Id);
            }

            return Enumerable.Empty<IdentityProviderProfile>();
        }

    }
}
