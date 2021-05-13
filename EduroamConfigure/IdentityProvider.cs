using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

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
        public GeoCoordinate GeoCoordinate { get => Geo.GeoCoordinate; }
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

        /// <summary>
        /// How the profile is shown to the end user
        /// </summary>
        /// <returns>Name of profile</returns>
        public override string ToString()
             => Name;
    }

    // Stores information found in IdentityProvider json. 
    public class IdentityProvider
    {
        public string Country { get; set; } // ISO2
        public string Name { get; set; }
        public List<IdpCoordinates> Geo { get; set; } = new List<IdpCoordinates>();
        public string Id { get; set; }
        public List<IdentityProviderProfile> Profiles { get; set; }
        public IEnumerable<GeoCoordinate> GeoCoordinates { get => Geo.Select((geo) => geo.GeoCoordinate); }

        public GeoCoordinate GetClosestGeoCoordinate(GeoCoordinate compareCoordinate)
        {
            var closestGeo = GeoCoordinate.Unknown;
            // shortest distance
            double shortestDistance = double.MaxValue;
            foreach (GeoCoordinate geo in GeoCoordinates)
            {
                double currentDistance = geo.GetDistanceTo(compareCoordinate);
                // compares with shortest distance
                if (currentDistance < shortestDistance)
                {
                    // sets the current distance as the shortest dstance
                    shortestDistance = currentDistance;
                    // sets inst with shortest distance to be the closest institute
                    closestGeo = geo;
                }
            }
            return closestGeo;
        }
        /// <summary>
        /// How the institution is shown to the end user
        /// </summary>
        /// <returns>Name of institution</returns>
        public override string ToString()
             => Name;

        internal double getDistanceTo(GeoCoordinate coordinates)
        {
            var closest = GetClosestGeoCoordinate(coordinates);
            return closest.IsUnknown
                ? double.MaxValue
                : coordinates.GetDistanceTo(closest);
        }
    }

    #pragma warning restore IDE1006 // Naming Styles
    #pragma warning restore CA2227 // Collection properties should be read only
}
