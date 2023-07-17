using Newtonsoft.Json;

namespace EduRoam.Connect.Store
{
    public class WifiEndpoint
    {
        public WifiEndpoint(string profileId, Uri tokenEndpoint, Uri EapEndpoint)
        {
            this.ProfileId = profileId;
            this.TokenEndpoint = tokenEndpoint;
            this.EapEndpoint = EapEndpoint;
        }

        [JsonProperty("profileId")]
        public string ProfileId { get; }

        [JsonProperty("tokenEndpoint")]
        public Uri TokenEndpoint { get; }

        [JsonProperty("eapEndpoint")]
        public Uri EapEndpoint { get; }
    }
}
