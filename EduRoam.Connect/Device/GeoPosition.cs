using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Contains location data of a type specified by the type parameter of the System.Device.Location.GeoPosition`1
    //     class.
    //
    // Type parameters:
    //   T:
    //     The type of the location data.
    public class GeoPosition<T>
    {
        private DateTimeOffset m_timestamp = DateTimeOffset.MinValue;

        private T m_position;

        //
        // Summary:
        //     Gets or sets the location data for the System.Device.Location.GeoPosition`1 object.
        //
        // Returns:
        //     An object of type T that contains the location data for the System.Device.Location.GeoPosition`1
        //     object.
        public T Location
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }

        //
        // Summary:
        //     Gets or sets the time when the location data was obtained.
        //
        // Returns:
        //     A System.DateTimeOffset that contains the time the location data was created.
        public DateTimeOffset Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Device.Location.GeoPosition`1 class.
        public GeoPosition()
            : this(DateTimeOffset.MinValue, default(T))
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Device.Location.GeoPosition`1 class
        //     with a timestamp and position.
        //
        // Parameters:
        //   timestamp:
        //     The time the location data was obtained.
        //
        //   position:
        //     The location data to use to initialize the System.Device.Location.GeoPosition`1
        //     object.
        public GeoPosition(DateTimeOffset timestamp, T position)
        {
            Timestamp = timestamp;
            Location = position;
        }
    }
}
