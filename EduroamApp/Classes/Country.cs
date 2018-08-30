using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
	/// <summary>
	/// Stores information about a country.
	/// Used for populating list of countries when selecting institution.
	/// </summary>
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
