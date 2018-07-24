using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{   
    public class IdProviderProfileAttributes
    {
        public int status { get; set; }
        public Data data { get; set; }
        public string tou { get; set; }

        public class Options
        {
            public int sign { get; set; }
            public string device_id { get; set; }
            public string mime { get; set; }
            public string args { get; set; }
            public int? hidden { get; set; }
            public int? redirect { get; set; }
            public string message { get; set; }
            public int? no_cache { get; set; }
        }

        public class Device
        {
            public string id { get; set; }
            public string display { get; set; }
            public int status { get; set; }
            public int redirect { get; set; }
            public int eap_customtext { get; set; }
            public int device_customtext { get; set; }
            public object message { get; set; }
            public Options options { get; set; }
        }

        public class Data
        {
            public string local_email { get; set; }
            public string local_phone { get; set; }
            public string local_url { get; set; }
            public List<Device> devices { get; set; }
        }
    }
}
