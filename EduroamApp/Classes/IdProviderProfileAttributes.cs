using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{   
    public class IdProviderProfileAttributes
    {
        public int Status { get; set; }
        public Datum Data { get; set; }
        public string Tou { get; set; }

        public class Options
        {
            public int Sign { get; set; }
            public string DeviceId { get; set; }
            public string Mime { get; set; }
            public string Args { get; set; }
            public int? Hidden { get; set; }
            public int? Redirect { get; set; }
            public string Message { get; set; }
            public int? NoCache { get; set; }
        }

        public class Device
        {
            public string Id { get; set; }
            public string Display { get; set; }
            public int Status { get; set; }
            public string Redirect { get; set; }
            public int EapCustomtext { get; set; }
            public int DeviceCustomtext { get; set; }
            public object Message { get; set; }
            public Options Options { get; set; }
        }

        public class Datum  
        {
            public string LocalEmail { get; set; }
            public string LocalPhone { get; set; }
            public string LocalUrl { get; set; }
            public List<Device> Devices { get; set; }
        }
    }
}
