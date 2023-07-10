using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Interface that can be implemented for providing accessing location data and receiving
    //     location updates.
    //
    // Type parameters:
    //   T:
    //     The type of the object that contains the location data.
    public interface IGeoPositionWatcher<T>
    {
        //
        // Summary:
        //     Gets the location data.
        //
        // Returns:
        //     The System.Device.Location.GeoPosition`1 containing the location data.
        GeoPosition<T> Position { get; }

        //
        // Summary:
        //     Gets the status of location data.
        //
        // Returns:
        //     The status of location data.
        GeoPositionStatus Status { get; }

        //
        // Summary:
        //     Occurs when the System.Device.Location.IGeoPositionWatcher`1.Position property
        //     has changed.
        event EventHandler<GeoPositionChangedEventArgs<T>> PositionChanged;

        //
        // Summary:
        //     Occurs when the System.Device.Location.IGeoPositionWatcher`1.Status property
        //     changes.
        event EventHandler<GeoPositionStatusChangedEventArgs> StatusChanged;

        //
        // Summary:
        //     Initiate the acquisition of location data.
        void Start();

        //
        // Summary:
        //     Start acquiring location data, specifying whether or not to suppress prompting
        //     for permissions. This method returns synchronously.
        //
        // Parameters:
        //   suppressPermissionPrompt:
        //     If true, do not prompt the user to enable location providers and only start if
        //     location data is already enabled. If false, a dialog box may be displayed to
        //     prompt the user to enable location sensors that are disabled.
        void Start(bool suppressPermissionPrompt);

        //
        // Summary:
        //     Start acquiring location data, specifying an initialization timeout. This method
        //     returns synchronously.
        //
        // Parameters:
        //   suppressPermissionPrompt:
        //     If true, do not prompt the user to enable location providers and only start if
        //     location data is already enabled. If false, a dialog box may be displayed to
        //     prompt the user to enable location sensors that are disabled.
        //
        //   timeout:
        //     Time in milliseconds to wait for initialization to complete.
        //
        // Returns:
        //     true if succeeded, false if timed out.
        bool TryStart(bool suppressPermissionPrompt, TimeSpan timeout);

        //
        // Summary:
        //     Stop acquiring location data.
        void Stop();
    }
}
