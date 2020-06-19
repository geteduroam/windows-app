using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    class DiscoveryApi
    {
        public int Version { get; set; }
        public int Seq { get; set; }
        public List<IdentityProvider> Instances { get; set; }

    }
}
