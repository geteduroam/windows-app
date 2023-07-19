using EduRoam.Connect.Device;

// GeoCoordinate => Coordinate
namespace EduRoam.Connect.Identity
{
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
            get => new GeoCoordinate(this.Lat, this.Lon);
        }
    }

}
