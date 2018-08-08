using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    class Country
    {
        // Properties
        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        // Constructor
        public Country(string countryCode, string countryName)
        {
            CountryCode = countryCode;
            CountryName = countryName;
        }
    }
}
