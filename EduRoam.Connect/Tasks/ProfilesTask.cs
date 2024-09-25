using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Store;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduRoam.Connect.Tasks
{
    public class ProfilesTask
    {
        private readonly BaseConfigStore store = new RegistryStore();

        /// <summary>
        /// Get a list of available institute profiles
        /// </summary>
        /// <param name="institute">Name of institute to get profiles for</param>
        /// <param name="query">Query to filter profiles</param>
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        public static async Task<IEnumerable<IdentityProviderProfile>> GetProfilesAsync(string institute, string? query = null)
        {
            if (string.IsNullOrWhiteSpace(institute))
            {
                throw new ArgumentNullException(nameof(institute));
            }

            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders();
            if (idpDownloader.Loaded)
            {
                var provider = idpDownloader.ClosestProviders.SingleOrDefault(provider => provider.Name.Equals(institute, StringComparison.InvariantCultureIgnoreCase));

                if (provider == null)
                {
                    throw new UnknownInstituteException(institute);
                }

                var profiles = idpDownloader.GetIdentityProviderProfiles(provider.Id);
                if (string.IsNullOrWhiteSpace(query))
                {
                    return profiles;
                }

                return profiles.Where(provider => provider.Name.ToUpper().Contains(query)); // somehow cannot use Contains(string, StringComparer.CurrentCultureIgnoreCase)

            }

            return Enumerable.Empty<IdentityProviderProfile>();
        }

        public static async Task<IdentityProviderProfile?> GetProfileAsync(string profileId)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders();
            return await idpDownloader.GetProfileFromId(profileId);
        }

        public string GetCurrentProfileName()
        {
            return this.store.IdentityProvider?.DisplayName ?? ApplicationResources.GetString("DefaultIdentityProvider");
        }

        public void RemoveCurrentProfile()
        {
            RemoveWiFiConfigurationTask.Remove(omitRootCa: true);
        }
    }
}
