using Geolocation;

namespace EduRoam.Connect.Device
{

    public class GeoCoordinate
    {
        public Coordinate? Coordinate
        {
            get;
        }

        private GeoCoordinate()
        {
        }

        public GeoCoordinate(double latitude, double longitude)
        {
            if (double.IsNaN(latitude))
            {
                throw new ArgumentNullException(nameof(latitude));
            }

            if (double.IsNaN(longitude))
            {
                throw new ArgumentNullException(nameof(longitude));
            }


            Coordinate = new Coordinate(latitude, longitude);
        }

        public bool IsUnknown => Coordinate == null;

        public static GeoCoordinate Unknown => new GeoCoordinate();


        public double GetDistanceTo(GeoCoordinate coordinate)
        {
            if (Coordinate == null || coordinate.Coordinate == null)
            {
                throw new ArgumentException("Latitude Or Longitude Is Not A Number");
            }

            return GeoCalculator.GetDistance(Coordinate.Value, coordinate.Coordinate.Value, 0, DistanceUnit.Meters);
        }
    }
}
