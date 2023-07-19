using EduRoam.Connect.Device;

// GeoCoordinate => Coordinate
namespace EduRoam.Connect.Identity
{
    public class IdpLocation
    {
        public string Country { get; set; }
        public string Postal { get; set; }
        public string City { get; set; }
        public IdpCoordinates Geo { get; set; }
        public GeoCoordinate GeoCoordinate { get => this.Geo.GeoCoordinate; }
    }
}
