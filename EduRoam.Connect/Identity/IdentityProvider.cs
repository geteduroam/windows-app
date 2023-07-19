using EduRoam.Connect.Device;

// GeoCoordinate => Coordinate
namespace EduRoam.Connect.Identity
{

    // Stores information found in IdentityProvider json.
    public class IdentityProvider
    {
        public string Country { get; set; } // ISO2
        public string Name { get; set; }
        public List<IdpCoordinates> Geo { get; set; } = new List<IdpCoordinates>();
        public string Id { get; set; }
        public List<IdentityProviderProfile> Profiles { get; set; }
        public IEnumerable<GeoCoordinate> GeoCoordinates { get => this.Geo.Select((geo) => geo.GeoCoordinate); }

        public GeoCoordinate GetClosestGeoCoordinate(GeoCoordinate compareCoordinate)
        {
            var closestGeo = GeoCoordinate.Unknown;
            // shortest distance
            var shortestDistance = double.MaxValue;
            foreach (var geo in this.GeoCoordinates)
            {
                var currentDistance = geo.GetDistanceTo(compareCoordinate);
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
             => this.Name;

        internal double getDistanceTo(GeoCoordinate coordinates)
        {
            var closest = this.GetClosestGeoCoordinate(coordinates);
            return closest.IsUnknown
                ? double.MaxValue
                : coordinates.GetDistanceTo(closest);
        }
    }
}
