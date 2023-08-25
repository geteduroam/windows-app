﻿using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Store;
using EduRoam.Localization;

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
        public async Task<IEnumerable<IdentityProviderProfile>> GetProfilesAsync(string institute, string? query = null)
        {
            if (string.IsNullOrWhiteSpace(institute))
            {
                throw new ArgumentNullException(nameof(institute));
            }

            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders(useGeodata: true);
            if (idpDownloader.Loaded)
            {
                var provider = idpDownloader.Providers.SingleOrDefault(provider => provider.Name.Equals(institute, StringComparison.InvariantCultureIgnoreCase));

                if (provider == null)
                {
                    throw new UnknownInstituteException(institute);
                }

                var profiles = idpDownloader.GetIdentityProviderProfiles(provider.Id);
                if (string.IsNullOrWhiteSpace(query))
                {
                    return profiles;
                }
                return profiles.Where(provider => provider.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase));

            }

            return Enumerable.Empty<IdentityProviderProfile>();
        }

        public static async Task<IdentityProviderProfile?> GetProfileAsync(string profileId)
        {
            using var idpDownloader = new IdentityProviderDownloader();

            await idpDownloader.LoadProviders(useGeodata: true);
            return idpDownloader.GetProfileFromId(profileId);
        }

        public string GetCurrentProfileName()
        {
            return this.store.IdentityProvider?.DisplayName ?? Resources.DefaultIdentityProvider;
        }

        public void RemoveCurrentProfile()
        {
            var remover = new RemoveWiFiConfigurationTask();
            remover.Remove(omitRootCa: true);
        }
    }
}
