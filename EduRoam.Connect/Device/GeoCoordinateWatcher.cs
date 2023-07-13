using System.ComponentModel;
using System.Security;

namespace EduRoam.Connect.Device
{
    //
    // Summary:
    //     Supplies location data that is based on latitude and longitude coordinates.
    [SecurityCritical]
    public class GeoCoordinateWatcher : IDisposable, INotifyPropertyChanged, IGeoPositionWatcher<GeoCoordinate>
    {
        private delegate void EventRaiser<T>(T e) where T : EventArgs;

        private GeoCoordinate m_lastCoordinate = GeoCoordinate.Unknown;

        private GeoPositionAccuracy m_desiredAccuracy;

        private GeoCoordinateWatcherInternal m_watcher;

        private PropertyChangedEventHandler m_propertyChanged;

        private EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> m_positionChanged;

        private EventHandler<GeoPositionStatusChangedEventArgs> m_statusChanged;

        private SynchronizationContext m_synchronizationContext;

        private bool m_disposed;

        private double m_threshold;

        //
        // Summary:
        //     The requested accuracy level for the location data that is provided by the System.Device.Location.GeoCoordinateWatcher.
        //
        // Returns:
        //     System.Device.Location.GeoPositionAccuracy, which indicates the requested accuracy
        //     level of the location provider.
        public GeoPositionAccuracy DesiredAccuracy
        {
            get
            {
                this.DisposeCheck();
                return this.m_desiredAccuracy;
            }
            private set
            {
                this.DisposeCheck();
                this.m_desiredAccuracy = value;
            }
        }

        //
        // Summary:
        //     The distance that must be moved, in meters, relative to the coordinate from the
        //     last System.Device.Location.GeoCoordinateWatcher.PositionChanged event, before
        //     the location provider raises another System.Device.Location.GeoCoordinateWatcher.PositionChanged
        //     event.
        //
        // Returns:
        //     Distance, in meters.
        public double MovementThreshold
        {
            get
            {
                this.DisposeCheck();
                return this.m_threshold;
            }
            set
            {
                this.DisposeCheck();
                if (value < 0.0 || double.IsNaN(value))
                {
                    throw new ArgumentOutOfRangeException("value", "Argument_MustBeNonNegative");
                }

                this.m_threshold = value;
            }
        }

        //
        // Summary:
        //     Indicates whether permission to access location data from location providers
        //     has been granted or denied.
        //
        // Returns:
        //     A value that indicates whether permission has been granted or denied.
        public GeoPositionPermission Permission
        {
            get
            {
                this.DisposeCheck();
                throw new NotSupportedException();
                // return m_watcher.Permission;
            }
        }

        //
        // Summary:
        //     Gets the System.Device.Location.GeoCoordinate which indicates the current location.
        //
        // Returns:
        //     The System.Device.Location.GeoCoordinate which indicates the current location.
        public GeoPosition<GeoCoordinate> Position
        {
            [SecuritySafeCritical]
            get
            {
                this.DisposeCheck();
                throw new NotSupportedException();
                // return m_watcher.Position;
            }
        }

        //
        // Summary:
        //     Gets the current status of the System.Device.Location.GeoCoordinateWatcher.
        //
        // Returns:
        //     A System.Device.Location.GeoPositionStatus which indicates the availability of
        //     data from the System.Device.Location.GeoCoordinateWatcher.
        public GeoPositionStatus Status
        {
            [SecuritySafeCritical]
            get
            {
                this.DisposeCheck();
                throw new NotSupportedException();
                // return m_watcher.Status;
            }
        }

        //
        // Summary:
        //     Indicates that the latitude or longitude of the location data has changed.
        public event EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> PositionChanged;

        //
        // Summary:
        //     Indicates that the status of the System.Device.Location.GeoCoordinateWatcher
        //     object has changed.
        public event EventHandler<GeoPositionStatusChangedEventArgs> StatusChanged;

        //
        // Summary:
        //     Indicates that the System.Device.Location.GeoCoordinateWatcher.Status property,
        //     the System.Device.Location.GeoCoordinateWatcher.Position property, or the System.Device.Location.GeoCoordinateWatcher.Permission
        //     property has changed.
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            [SecuritySafeCritical]
            add
            {
                this.m_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(this.m_propertyChanged, value);
            }
            [SecuritySafeCritical]
            remove
            {
                this.m_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(this.m_propertyChanged, value);
            }
        }

        //
        // Summary:
        //     Indicates that the location data has changed.
        event EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> IGeoPositionWatcher<GeoCoordinate>.PositionChanged
        {
            [SecuritySafeCritical]
            add
            {
                this.m_positionChanged = (EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>)Delegate.Combine(this.m_positionChanged, value);
            }
            [SecuritySafeCritical]
            remove
            {
                this.m_positionChanged = (EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>)Delegate.Remove(this.m_positionChanged, value);
            }
        }

        //
        // Summary:
        //     Indicates that the status of the location provider has changed.
        event EventHandler<GeoPositionStatusChangedEventArgs> IGeoPositionWatcher<GeoCoordinate>.StatusChanged
        {
            [SecuritySafeCritical]
            add
            {
                this.m_statusChanged = (EventHandler<GeoPositionStatusChangedEventArgs>)Delegate.Combine(this.m_statusChanged, value);
            }
            [SecuritySafeCritical]
            remove
            {
                this.m_statusChanged = (EventHandler<GeoPositionStatusChangedEventArgs>)Delegate.Remove(this.m_statusChanged, value);
            }
        }

        //
        // Summary:
        //     Initializes a new instance of System.Device.Location.GeoCoordinateWatcher with
        //     default accuracy settings.
        public GeoCoordinateWatcher()
            : this(GeoPositionAccuracy.Default)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of System.Device.Location.GeoCoordinateWatcher, given
        //     an accuracy level.
        //
        // Parameters:
        //   desiredAccuracy:
        //     System.Device.Location.GeoPositionAccuracy that indicates the requested accuracy
        //     level of the location provider. An accuracy of System.Device.Location.GeoPositionAccuracy.High
        //     can degrade performance and should be specified only when high accuracy is needed.
        public GeoCoordinateWatcher(GeoPositionAccuracy desiredAccuracy)
        {
            this.m_desiredAccuracy = desiredAccuracy;
            this.m_watcher = new GeoCoordinateWatcherInternal(desiredAccuracy);
            if (SynchronizationContext.Current == null)
            {
                this.m_synchronizationContext = new SynchronizationContext();
            }
            else
            {
                this.m_synchronizationContext = SynchronizationContext.Current;
            }

            this.m_watcher.StatusChanged += this.OnInternalStatusChanged;
            this.m_watcher.PermissionChanged += this.OnInternalPermissionChanged;
            this.m_watcher.PositionChanged += this.OnInternalLocationChanged;
        }

        //
        // Summary:
        //     Initiate the acquisition of data from the current location provider. This method
        //     enables System.Device.Location.GeoCoordinateWatcher.PositionChanged events and
        //     allows access to the System.Device.Location.GeoCoordinateWatcher.Position property.
        [SecuritySafeCritical]
        public void Start()
        {
            this.DisposeCheck();
            this.Start(suppressPermissionPrompt: false);
        }

        //
        // Summary:
        //     Initiate the acquisition of data from the current location provider. This method
        //     enables System.Device.Location.GeoCoordinateWatcher.PositionChanged events and
        //     allows access to the System.Device.Location.GeoCoordinateWatcher.Position property.
        //
        // Parameters:
        //   suppressPermissionPrompt:
        //     true to suppress the permission dialog box; false to optionally show the permission
        //     dialog box if permissions have not already been granted.
        [SecuritySafeCritical]
        public void Start(bool suppressPermissionPrompt)
        {
            this.DisposeCheck();
            throw new NotSupportedException();
            // m_watcher.TryStart(suppressPermissionPrompt, TimeSpan.Zero);
        }

        //
        // Summary:
        //     Initiates the acquisition of data from the current location provider. This method
        //     returns synchronously.
        //
        // Parameters:
        //   suppressPermissionPrompt:
        //     true to suppress the permission dialog box; false to display the permission dialog
        //     box.
        //
        //   timeout:
        //     Time in milliseconds to wait for the location provider to start before timing
        //     out.
        //
        // Returns:
        //     true if data acquisition is started within the time period specified by timeout;
        //     otherwise, false.
        [SecuritySafeCritical]
        public bool TryStart(bool suppressPermissionPrompt, TimeSpan timeout)
        {
            this.DisposeCheck();
            var num = (long)timeout.TotalMilliseconds;
            if (num <= 0 || int.MaxValue < num)
            {
                throw new NotSupportedException();
                // return m_watcher.IsStarted;
            }

            throw new NotSupportedException();
            // return m_watcher.TryStart(suppressPermissionPrompt, timeout);
        }

        //
        // Summary:
        //     Stops the System.Device.Location.GeoCoordinateWatcher from providing location
        //     data and events.
        [SecuritySafeCritical]
        public void Stop()
        {
            this.DisposeCheck();
            throw new NotSupportedException();
            // m_watcher.Stop();
        }

        private void OnInternalLocationChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position != null && (this.m_lastCoordinate == GeoCoordinate.Unknown || e.Position.Location == GeoCoordinate.Unknown || e.Position.Location.GetDistanceTo(this.m_lastCoordinate) >= this.m_threshold))
            {
                this.m_lastCoordinate = e.Position.Location;
                this.PostEvent(this.OnPositionChanged, new GeoPositionChangedEventArgs<GeoCoordinate>(e.Position));
                this.OnPropertyChanged("Position");
            }
        }

        private void OnInternalStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            this.PostEvent(this.OnPositionStatusChanged, new GeoPositionStatusChangedEventArgs(e.Status));
            this.OnPropertyChanged("Status");
        }

        private void OnInternalPermissionChanged(object sender, GeoPermissionChangedEventArgs e)
        {
            this.OnPropertyChanged("Permission");
        }

        //
        // Summary:
        //     Called when a System.Device.Location.GeoCoordinateWatcher.PositionChanged event
        //     occurs.
        //
        // Parameters:
        //   e:
        //     A System.Device.Location.GeoPositionChangedEventArgs`1 object that contains the
        //     new location.
        protected void OnPositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.PositionChanged?.Invoke(this, e);
        }

        //
        // Summary:
        //     Called when a System.Device.Location.GeoCoordinateWatcher.StatusChanged event
        //     occurs.
        //
        // Parameters:
        //   e:
        //     A System.Device.Location.GeoPositionStatusChangedEventArgs object that contains
        //     the new status.
        protected void OnPositionStatusChanged(GeoPositionStatusChangedEventArgs e)
        {
            this.StatusChanged?.Invoke(this, e);
        }

        //
        // Summary:
        //     Called when a property of the System.Device.Location.GeoCoordinateWatcher changes.
        //
        // Parameters:
        //   propertyName:
        //     The name of the property that has changed.
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.m_propertyChanged != null)
            {
                this.m_propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //
        // Summary:
        //     Releases all resources that are used by the current instance of the System.Device.Location.GeoCoordinateWatcher
        //     class.
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        //
        // Summary:
        //     Frees resources and performs other cleanup operations before the System.Device.Location.GeoCoordinateWatcher
        //     is reclaimed by garbage collection.
        [SecuritySafeCritical]
        ~GeoCoordinateWatcher()
        {
            this.Dispose(disposing: false);
        }

        //
        // Summary:
        //     Releases all resources used by the current instance of the System.Device.Location.GeoCoordinateWatcher
        //     class.
        //
        // Parameters:
        //   disposing:
        //     true to release both managed and unmanaged resources; false to release only unmanaged
        //     resources.
        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_disposed)
            {
                //if (disposing && m_watcher != null)
                //{
                //    m_watcher.Dispose();
                //    m_watcher = null;
                //}

                this.m_disposed = true;
            }
        }

        private void DisposeCheck()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("GeoCoordinateWatcher");
            }
        }

        private void PostEvent<T>(EventRaiser<T> callback, T e) where T : EventArgs
        {
            this.m_synchronizationContext.Post(delegate (object state)
            {
                callback((T)state);
            }, e);
        }
    }
}
