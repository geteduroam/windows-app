using System.Collections.Generic;
using System.Linq;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Identity.v2;

namespace EduRoam.Connect.Converter
{
    /// <summary>
    /// Converts the discovery v2 structure to discovery v1 structure
    /// </summary>
    public static class DiscoveryConverter
    {
        public static DiscoveryApi Covert(LetsWifiDiscovery input)
        {
            var output = new DiscoveryApi
            {
                Version = "2",
                Seq = input.Root.Seq,
                Instances = input.Root.Institutions.Select(institution => new IdentityProvider
                {
                    Country = institution.Country,
                    Id = institution.Id,
                    Name = institution.Name["any"],
                    SearchTags = PopulateSearchTags(institution),
                    Profiles = institution.Profiles.Select(profile => new IdentityProviderProfile
                    {
                        Name = profile.Name.ContainsKey("any") ? profile.Name["any"] : institution.Name["any"],
                        Id = profile.Id,
                        OAuth = profile.Type == "letswifi",
                        EapConfigEndpoint = profile.Type == "eap-config" ? profile.EapConfigEndpoint : null,
                        Redirect = profile.Type == "webview" ? profile.WebViewEndpoint : null,
                        LetsWifiEndpoint = profile.LetsWifiEndpoint,
                    }).ToList()
                }).ToList()
            };
            
            return output;
        }

        private static List<string> PopulateSearchTags(LetsWifiDiscovery.DiscoveryInstitution institution)
        {
            List<string> searchTags = [];

            searchTags.AddRange(institution.Name.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList());

            if(institution.Profiles.Any(p => p.LetsWifiEndpoint != null))
            {
                var profiles = institution.Profiles.Where(p => p.LetsWifiEndpoint != null);
                searchTags.AddRange(profiles.Select(p => p.LetsWifiEndpoint).ToList());
            }

            return searchTags;
        }
    }
}