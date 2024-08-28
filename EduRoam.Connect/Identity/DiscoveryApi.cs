using System.Collections.Generic;

namespace EduRoam.Connect.Identity
{
    public class DiscoveryApi
    {
        public DiscoveryApi()
        {
            this.Version = "";
            this.Seq = "";
            this.Instances = new List<IdentityProvider>();
        }

        public string Version { get; set; }
        public string Seq { get; set; }
        public List<IdentityProvider> Instances { get; set; }
    }
}