using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Specifies the requested accuracy level for the location data that the application
    //     uses.
    public enum GeoPositionAccuracy
    {
        //
        // Summary:
        //     Optimize for power, performance, and other cost considerations.
        Default,
        //
        // Summary:
        //     Deliver the most accurate report possible. This includes using services that
        //     might charge money, or consuming higher levels of battery power or connection
        //     bandwidth.
        High
    }
}
