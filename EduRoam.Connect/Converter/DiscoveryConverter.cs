using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Identity.v2;

using static EduRoam.Connect.Identity.v2.LetsWifiDiscovery;

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
                Instances = input.Root.Providers.Select(provider => new IdentityProvider
                {
                    Country = provider.Country,
                    Id = provider.Id,
                    Name = translate(provider.Name),
                    SearchTags = PopulateSearchTags(provider),
                    Profiles = provider.Profiles.Select(profile => new IdentityProviderProfile
                    {
                        Name = profile.Name == null || profile.Name.Count == 0 ? translate(provider.Name) : translate(profile.Name),
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

        private static string translate(List<DiscoveryName> name)
        {
            // TODO improve with GlobalizationPreferences.Languages if .NET 9 is available
            // https://learn.microsoft.com/en-us/uwp/api/windows.system.userprofile.globalizationpreferences.languages
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            foreach (var translatedName in name)
            {
                if (cultureInfo.Parent.Name == translatedName.Lang)
                    return translatedName.Display;
            }
            return name.First().Display;
        }

        private static List<string> PopulateSearchTags(LetsWifiDiscovery.DiscoveryInstitution provider)
        {
            List<string> searchTags = [];

            searchTags.AddRange(provider.Name.Where(x => !string.IsNullOrEmpty(x.Display)).Select(x => x.Display).ToList());

            if(provider.Profiles.Any(p => p.LetsWifiEndpoint != null))
            {
                var profiles = provider.Profiles.Where(p => p.LetsWifiEndpoint != null);
                searchTags.AddRange(profiles.Select(p => p.LetsWifiEndpoint!).ToList());
            }

            return searchTags;
        }
    }
}