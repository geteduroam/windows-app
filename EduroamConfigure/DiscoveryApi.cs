using System.Collections.Generic;

namespace EduroamConfigure
{
    #pragma warning disable CA2227 // Collection properties should be read only
    // TODO: make into subclass, concider struct
    public class DiscoveryApi
    {
        public int Version { get; set; }
        public int Seq { get; set; }
        public List<IdentityProvider> Instances { get; set; }
    }
    #pragma warning restore CA2227 // Collection properties should be read only
}
