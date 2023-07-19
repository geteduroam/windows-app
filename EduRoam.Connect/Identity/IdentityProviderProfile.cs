using Newtonsoft.Json;

namespace EduRoam.Connect.Identity
{
    public class IdentityProviderProfile
    {
        public string Id { get; set; }

        [JsonProperty("cat_profile")]
        public int CatProfile { get; set; }

        public string Name { get; set; }

        [JsonProperty("eapconfig_endpoint")]
        public string EapConfigEndpoint { get; set; }

        [JsonProperty("oauth")]
        public bool OAuth { get; set; }

        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonProperty("redirect")]
        public string Redirect { get; set; }

        /// <summary>
        /// How the profile is shown to the end user
        /// </summary>
        /// <returns>Name of profile</returns>
        public override string ToString() => this.Name;
    }
}
