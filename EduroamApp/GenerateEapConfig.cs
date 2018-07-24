using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{       
    public class GenerateEapConfig
    {
        public int status { get; set; }
        public Data data { get; set; }
        public string tou { get; set; }

        public class Data
        {
            public string profile { get; set; }
            public string device { get; set; }
            public string link { get; set; }
            public string mime { get; set; }
        }
    }
}
