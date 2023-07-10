using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    internal class GeoCoordinateWatcherInternal
    {
        public GeoCoordinateWatcherInternal(GeoPositionAccuracy desiredAccuracy)
        {
            DesiredAccuracy = desiredAccuracy;
        }

        public GeoPositionAccuracy DesiredAccuracy { get; }
        public Action<object, GeoPositionStatusChangedEventArgs> StatusChanged { get; internal set; }
        public Action<object, GeoPermissionChangedEventArgs> PermissionChanged { get; internal set; }
        public Action<object, GeoPositionChangedEventArgs<GeoCoordinate>> PositionChanged { get; internal set; }
    }
}
