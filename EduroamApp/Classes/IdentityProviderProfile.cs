using System.Collections.Generic;

namespace EduroamApp
{
	/// <summary>
	/// Stores information found in json for an IdentityProvider profile.
	/// </summary>
	public class IdentityProviderProfile
	{
		// Properties
		public int Status { get; set; }
		public List<Datum> Data { get; set; }
		public string Tou { get; set; }

		// Contains profile data.
		public class Datum
		{
			// Properties
			public string Id { get; set; }
			public string Display { get; set; }
			public string IdpName { get; set; }
			public bool Logo { get; set; }
		}
	}
}
