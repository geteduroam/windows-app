using System.Collections.Generic;
using System.Device.Location;

namespace EduroamConfigure
{
	// since these are json field:
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA2227 // Collection properties should be read only

	/// <summary>
	/// Stores geographical coordinates.
	/// </summary>
	public class IdpCoordinates
	{
		// Properties
		public double Lon { get; set; }
		public double Lat { get; set; }

		public GeoCoordinate GeoCoordinate
		{
			get => new GeoCoordinate(Lat, Lon);
		}
	}

	public class IdpLocation
	{
		public string Country { get; set; }
		public string Postal { get; set; }
		public string City { get; set; }
		public IdpCoordinates Geo { get; set; }
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
		public string redirect { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	// Stores information found in IdentityProvider json.
	public class IdentityProvider
	{
		public string Country { get; set; } // ISO2
		public string Name { get; set; }
		public List<IdpCoordinates> Geo { get; set; }
		public int cat_idp { get; set; }
		public List<IdentityProviderProfile> Profiles { get; set; }

		public GeoCoordinate GetClosestGeoCoordinate(GeoCoordinate compareCoordinate)
		{
			var closestGeo = new IdpCoordinates();
			// shortest distance
			double shortestDistance = double.MaxValue;
			foreach (IdpCoordinates geo in Geo)
			{
				double currentDistance = geo.GeoCoordinate.GetDistanceTo(compareCoordinate);
				// compares with shortest distance
				if (currentDistance < shortestDistance)
				{
					// sets the current distance as the shortest dstance
					shortestDistance = currentDistance;
					// sets inst with shortest distance to be the closest institute
					closestGeo = geo;
				}
			}
			return closestGeo.GeoCoordinate;
		}

		public override string ToString()
		{
			return Name;
		}

	}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA2227 // Collection properties should be read only
}
