using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Indicates the ability of the location provider to provide location updates.
    public enum GeoPositionStatus
    {
        //
        // Summary:
        //     A location provider is ready to supply new data.
        Ready,
        //
        // Summary:
        //     The location provider is initializing. For example, a GPS that is still obtaining
        //     a fix has this status.
        Initializing,
        //
        // Summary:
        //     No location data is available from any location provider.
        NoData,
        //
        // Summary:
        //     The location provider is disabled. On Windows 7, this is the case when the Sensor
        //     and Location platform has been disabled by group policy.
        Disabled
    }
}
