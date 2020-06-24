using System.Collections.Generic;

namespace EduroamApp
{
    class DiscoveryApi
    {
        public int Version { get; set; }
        public int Seq { get; set; }
        public List<IdentityProvider> Instances { get; set; }

    }
}
