using System.Collections.Generic;

using Newtonsoft.Json;

using Semver;

namespace EduRoam.Connect.Identity.v2
{
    public class LetsWifiDiscovery
    {
        [JsonProperty("http://letswifi.app/discovery#v3")]
        public DiscoveryRoot Root { get; set; } = null!;

        public class DiscoveryRoot
        {
            public List<DiscoveryInstitution> Providers { get; set; } = new();
            public Dictionary<string, SemVersion> MinimalAppVersion = new();
            public string Seq { get; set; } = null!;
        }
        
        public class DiscoveryInstitution
        {
            public string Id { get; set; } = null!;
            public string? Country { get; set; }
            public List<DiscoveryName> Name { get; set; } = new();
            public List<InstitutionProfile> Profiles { get; set; } = new();
        }

        public class DiscoveryName
        {
            public string Display { get; set; } = null!;
            public string Lang { get; set; } = null!;
        }

        public class InstitutionProfile
        {
            public string Id { get; set; } = null!;
            public string Type { get; set; } = null!;
            public List<DiscoveryName> Name { get; set; } = new();

            
            [JsonProperty("eapconfig_endpoint")]
            public string? EapConfigEndpoint { get; set; }
            [JsonProperty("mobileconfig_endpoint")]
            public string? MobileConfigEndpoint { get; set; }
            [JsonProperty("webview_endpoint")]
            public string? WebViewEndpoint { get; set; }
            [JsonProperty("letswifi_endpoint")]
            public string? LetsWifiEndpoint { get; set; }
        }
    }
}