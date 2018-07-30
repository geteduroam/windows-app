using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    public class IdentityProviderProfile
    {
        public int Status { get; set; }
        public List<Datum> Data { get; set; }
        public string Tou { get; set; }
        
        public class Datum
        {
            public string Id { get; set; }
            public string Display { get; set; }
            public string IdpName { get; set; }
            public int Logo { get; set; }
        }
    }
}
