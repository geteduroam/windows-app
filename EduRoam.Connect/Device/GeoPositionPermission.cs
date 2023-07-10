using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Indicates whether the calling application has permission to access location data.
    public enum GeoPositionPermission
    {
        //
        // Summary:
        //     Location permissions are not known. This status can occur while the provider
        //     is being initialized.
        Unknown,
        //
        // Summary:
        //     Location permissions are granted.
        Granted,
        //
        // Summary:
        //     Location permissions are denied.
        Denied
    }
}
