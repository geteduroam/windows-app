using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    public class IdentityProviderProfile
    {
        public int status { get; set; }
        public List<Datum> data { get; set; }
        public string tou { get; set; }

        public class Datum
        {
            public string id { get; set; }
            public string display { get; set; }
            public string idp_name { get; set; }
            public int logo { get; set; }
        }
    }
}
