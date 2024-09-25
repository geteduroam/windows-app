using Newtonsoft.Json;
namespace EduRoam.Connect.Identity.v2;
public class LetsWifiProfile
{
    [JsonProperty("http://letswifi.app/api#v2")]
    public ProfileRoot Root { get; set; } = null!;

    public class ProfileRoot
    {
        [JsonProperty("eapconfig_endpoint")]
        public string? EapConfigEndpoint { get; set; }
        
        [JsonProperty("mobileconfig_endpoint")]
        public string? MobileConfigEndpoint { get; set; }

        [JsonProperty("token_endpoint")]
        public string? TokenEndpoint { get; set; }

        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = null!;
    }
}

