using System.Collections.Generic;

namespace EduroamApp
{
	/// <summary>
	/// Stores geographical coordinates.
	/// </summary>
	public class Geo
	{
		// Properties
		public double Lon { get; set; }
		public double Lat { get; set; }
	}

	// Stores information found in IdentityProvider json.
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
