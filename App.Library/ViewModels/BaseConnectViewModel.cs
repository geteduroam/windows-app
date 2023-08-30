using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;
using TaskStatus = EduRoam.Connect.Tasks.TaskStatus;

namespace App.Library.ViewModels
{
    internal abstract class BaseConnectViewModel : BaseViewModel
    {
        protected readonly EapConfig eapConfig;

        protected readonly IConnection connection;

        protected TaskStatus? connectionStatus;

        public BaseConnectViewModel(MainViewModel owner, EapConfig eapConfig, IConnection connection)
            : base(owner)
        {
            this.eapConfig = eapConfig;
            this.connection = connection;
        }

        public override string PreviousTitle => SharedResources.ButtonClose;

        public override string NextTitle => SharedResources.ButtonConnect;

        protected override bool CanNavigateNextAsync()
        {
            return true;
        }

        protected override Task NavigateNextAsync()
        {
            return this.ConnectAsync();
        }

        protected override Task NavigatePreviousAsync()
        {
            this.Owner.CloseApp();

            return Task.CompletedTask;
        }

        public string Status
        {
            get
            {
                if (this.connectionStatus == null)
                {
                    return "";
                }
                if (this.connectionStatus.Success)
                {
                    return string.Join("\n", this.connectionStatus.Messages);
                }
                return string.Join("\n", this.connectionStatus.Errors.Concat(this.connectionStatus.Warnings));
            }
        }

        protected async Task ConnectAsync()
        {
            // Connect
            try
            {
                IList<string> messages = new List<string>();

                await this.ConfigureAndConnectAsync(messages);

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

            this.CallPropertyChanged(nameof(this.Status));
        }

        protected abstract Task ConfigureAndConnectAsync(IList<string> messages);

        public bool Connected => this.connectionStatus?.Success ?? false;

        public TaskStatus? ConnectionStatus => this.connectionStatus;
    }
}
