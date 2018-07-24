using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{   
    public class IdentityProvider
    {
        public int entityID { get; set; }
        public string country { get; set; }
        public int icon { get; set; }
        public string title { get; set; }
        public List<Geo> geo { get; set; }
        public int id { get; set; }

        public class Geo
        {
            public string lon { get; set; }
            public string lat { get; set; }
        }
    }
}
