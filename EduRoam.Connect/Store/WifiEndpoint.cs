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

        [JsonProperty("Item1")] // For backward compatibility
        public string ProfileId { get; }

        [JsonProperty("Item2")] // For backward compatibility
        public Uri TokenEndpoint { get; }

        [JsonProperty("Item3")] // For backward compatibility
        public Uri EapEndpoint { get; }
    }
}
