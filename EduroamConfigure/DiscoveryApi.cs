using System.Collections.Generic;

namespace EduroamConfigure
{
    public class DiscoveryApi
    {
        public int Version { get; set; }
        public int Seq { get; set; }
        public List<IdentityProvider> Instances { get; set; }

    }
}
