using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
	public class Geo
	{
		public double Lon { get; set; }
		public double Lat { get; set; }
	}

	public class IdentityProvider
	{
		public int EntityID { get; set; }
		public string Country { get; set; }
		public int Icon { get; set; }
		public string Title { get; set; }
		public List<Geo> Geo { get; set; }
		public int Id { get; set; }
	}
}
