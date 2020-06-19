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
	public class IdentityProviderProfile
	{
		public string Id { get; set; }
		public int cat_profile { get; set; }
		public string Name { get; set; }
		public string eapconfig_endpoint { get; set; }
		public bool oauth { get; set; }
		public string token_endpoint { get; set; }
		public string authorization_endpoint { get; set; }
	}

	// Stores information found in IdentityProvider json.
	public class IdentityProvider
	{
		public string Country { get; set; }
		public string Name { get; set; }
		public List<Geo> Geo { get; set; }
		public int cat_idp { get; set; }
		public List<IdentityProviderProfile> Profiles { get; set; }
	}

}
