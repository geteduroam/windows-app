using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.ViewModels
{
    public class ConnectViewModel : BaseViewModel
    {
        private readonly EapConfig eapConfig;
        private readonly DefaultConnection connection;

        private TaskStatus? connectionStatus;

        public ConnectViewModel(MainViewModel owner, EapConfig eapConfig, DefaultConnector connector)
            : base(owner)
        {
            this.eapConfig = eapConfig;
            this.connection = new DefaultConnection(connector);
        }

        protected override bool CanNavigateNextAsync()
        {
            return true;
        }

        public IEnumerable<string> Messages
        {
            get
            {
                Debug.WriteLine(string.Join(',', this.connectionStatus?.Messages ?? Array.Empty<string>()));
                return this.connectionStatus?.Messages ?? Array.Empty<string>();
            }
        }

        protected override async Task NavigateNextAsync()
        {
            // Connect
            try
            {
                IList<string> messages = new List<string>();
                var connectionProperties = new ConnectionProperties();

                this.connectionStatus = await this.connection.ConfigureAndConnectAsync(connectionProperties);

            }
            catch (EduroamAppUserException ex)
            {
                // TODO, NICE TO HAVE: log the error
                Debug.WriteLine(Resources.ErrorNoConnection, ex.UserFacingMessage);
                this.connectionStatus = TaskStatus.AsFailure(ex.UserFacingMessage);
            }

            catch (ArgumentException exc)
            {
                Debug.WriteLine(exc.Message);
                this.connectionStatus = TaskStatus.AsFailure(exc.Message);
            }
            catch (ApiParsingException e)
            {
                // Must never happen, because if the discovery is reached,
                // it must be parseable. Logging has been done upstream.
                Debug.WriteLine(Resources.ErrorApi);
                Debug.WriteLine(e.Message, e.GetType().ToString());
                this.connectionStatus = TaskStatus.AsFailure(Resources.ErrorApi);
            }
            catch (ApiUnreachableException)
            {
                Debug.WriteLine(Resources.ErrorNoInternet);
                this.connectionStatus = TaskStatus.AsFailure(Resources.ErrorNoInternet);
            }
            catch (Exception exc)
            {
                this.connectionStatus = TaskStatus.AsFailure(exc.Message);
            }

            this.CallPropertyChanged();
        }

        public bool Connected => this.connectionStatus?.Success ?? false;

        public TaskStatus? ConnectionStatus => this.connectionStatus;

    }
}