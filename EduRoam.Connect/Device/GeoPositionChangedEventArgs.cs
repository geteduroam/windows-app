using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Provides data for the System.Device.Location.GeoCoordinateWatcher.PositionChanged
    //     event.
    //
    // Type parameters:
    //   T:
    //     The type of the location data in the System.Device.Location.GeoPosition`1.Location
    //     property of this event's System.Device.Location.GeoPositionChangedEventArgs`1.Position
    //     property.
    public class GeoPositionChangedEventArgs<T> : EventArgs
    {
        //
        // Summary:
        //     Gets the location data associated with the event.
        //
        // Returns:
        //     A System.Device.Location.GeoPosition`1 object that contains the location data
        //     in its System.Device.Location.GeoPosition`1.Location property.
        public GeoPosition<T> Position { get; private set; }

        //
        // Summary:
        //     Initializes a new instance of the System.Device.Location.GeoPositionChangedEventArgs`1
        //     class
        //
        // Parameters:
        //   position:
        //     The updated position.
        public GeoPositionChangedEventArgs(GeoPosition<T> position)
        {
            Position = position;
        }
    }
}
