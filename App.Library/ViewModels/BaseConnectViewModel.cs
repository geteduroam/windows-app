using App.Library.Connections;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Localization;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
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

        public override string PageTitle => SharedResources.LoginTitle;

        public override string PreviousTitle => SharedResources.ButtonClose;

        public override string NextTitle => SharedResources.ButtonConnect;

        public override bool ShowNavigatePrevious => this.connectionStatus?.Success == true;

        public override bool ShowNavigateNext => this.connectionStatus == null || !this.connectionStatus.Success;

        protected override Task NavigatePreviousAsync()
        {
            this.Owner.CloseApp();

            return Task.CompletedTask;
        }

        protected override bool CanNavigateNextAsync()
        {
            return this.connectionStatus == null;
        }

        protected override Task NavigateNextAsync()
        {
            return this.ConnectAsync();
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
            catch (EduroamAppUserException eauExc)
            {
                this.Owner.Logger.LogError(eauExc, Resources.ErrorNoConnection, eauExc.UserFacingMessage);
                this.connectionStatus = TaskStatus.AsFailure(eauExc.UserFacingMessage);
            }

            catch (ArgumentException exc)
            {
                this.Owner.Logger.LogError(exc, exc.Message);
                this.connectionStatus = TaskStatus.AsFailure(exc.Message);
            }
            catch (ApiParsingException apExc)
            {
                // Must never happen, because if the discovery is reached,
                // it must be parseable. Logging has been done upstream.
                this.Owner.Logger.LogError(Resources.ErrorApi);
                this.Owner.Logger.LogError(apExc, apExc.Message);
                this.connectionStatus = TaskStatus.AsFailure(Resources.ErrorApi);
            }
            catch (ApiUnreachableException auExc)
            {
                this.Owner.Logger.LogError(auExc, Resources.ErrorNoInternet);
                this.connectionStatus = TaskStatus.AsFailure(Resources.ErrorNoInternet);
            }
            catch (Exception exc)
            {
                this.Owner.Logger.LogError(exc, "Cannot connect");
                this.connectionStatus = TaskStatus.AsFailure(exc.Message);
            }

            this.CallPropertyChanged(nameof(this.Status));
        }

        protected abstract Task ConfigureAndConnectAsync(IList<string> messages);

        public bool Connected => this.connectionStatus?.Success ?? false;

        public TaskStatus? ConnectionStatus => this.connectionStatus;
    }
}
