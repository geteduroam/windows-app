using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Contains data for a GeoPositionStatusChanged event.
    public class GeoPositionStatusChangedEventArgs : EventArgs
    {
        //
        // Summary:
        //     Gets the updated status.
        //
        // Returns:
        //     The updated status.
        public GeoPositionStatus Status { get; private set; }

        //
        // Summary:
        //     Initializes a new instance of the GeoPositionStatusChangedEventArgs class.
        //
        // Parameters:
        //   status:
        //     The new status.
        public GeoPositionStatusChangedEventArgs(GeoPositionStatus status)
        {
            Status = status;
        }
    }
}
