using App.Library.Command;
using App.Library.Utility;
using App.Library.Install;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using NETWORKLIST;
using Semver;
using App.Library.Tasks;
using EduRoam.Localization;

namespace App.Library.ViewModels
{
#pragma warning disable CA1822 // Members are bound by a template and therefore cannot be static
    public class MainViewModel : NotifyPropertyChanged, IDisposable
    {
        public static readonly SelfInstaller SelfInstaller = SelfInstaller.DefaultInstance;

        private Status status;

        private readonly INetworkListManager networkListManager;
        public bool ShowNotificationBar { get; set; } = false;
        public string NotificationButtonCaption { get; set; } = "";
        public bool UpdateAvailable { get; set; } = false;
        public bool NotificationDismiss { get; set; } = false;
        public bool SelfTestSuccess { get; set; } = false;
        public string NotificationText { get; set; } = "";

        public MainViewModel(ILogger<MainViewModel> logger)
        {
            this.ApplicationTitle = Settings.Settings.ApplicationName;
            this.LoadEapFileCommand = new DelegateCommand(this.GetEapFileFromDialog);
            this.RefreshCommand = new AsyncCommand(this.RefreshAsync);
            this.ReauthenticateCommand = new DelegateCommand(this.Reauthenticate);
            this.RemoveProfileCommand = new DelegateCommand(this.RemoveProfile);
            this.RemoveCertificatesCommand = new DelegateCommand(this.RemoveCertificates);
            this.UninstallCommand = new DelegateCommand(this.Uninstall);
            this.OpenHelpCommand = new DelegateCommand(this.OpenHelp);
            this.OpenSystemMenuCommand = new DelegateCommand(this.OpenSystemMenu);
            this.CancelUpdateCommand = new DelegateCommand(this.DismissNotification);
            this.OpenUpdateMenuCommand = new DelegateCommand(this.OpenUpdateMenu);
            this.DoUpdateCommand = new DelegateCommand(this.OnConfirmUpdate);
            this.OpenBrowserCommand = new DelegateCommand(this.OnOpenBrowser);
            this.CopyLinkCommand = new DelegateCommand(this.OnCopyLink);

            // This is updated to the relevant command
            this.NotificationCommand = new DelegateCommand(() => { });

            this.State = new ApplicationState();

            this.status = new StatusTask().GetStatus();
            this.IsLoading = true;
            this.IsConnected = false;

            this.Logger = logger;
            this.networkListManager = new NetworkListManager();

            this.Logger.LogInformation($"{this.AppTitle}, version: {this.AppVersion}, run as admin: {StatusTask.RunAsAdministrator}");

            var eapConfigFile = string.IsNullOrEmpty(Settings.Settings.EapConfigFileLocation)
                ? EapConfigTask.GetBundledEapConfigFile()
                : Settings.Settings.EapConfigFileLocation
                ;
            if (!string.IsNullOrEmpty(eapConfigFile))
            {
                this.LoadEapFile(eapConfigFile!);
            }
            else
            {
                this.SetActiveContent(new StatusViewModel(this));
            }

            this.VersionCheck();
            Task.Run(this.checkForUpdates);
        }
        private async void checkForUpdates()
        {
            await UpdateChecker.CheckIfUpdateAvailableAsync();
            this.VersionCheck();
            this.CallPropertyChanged(string.Empty);
            DelegateCommand.RaiseCanExecuteChanged();
        }

        public string ApplicationTitle { get; set; }

        public bool IsConnected { get; set; }

        public ApplicationState State { get; private set; }

        public BaseViewModel? ActiveContent { get; private set; }

        public DelegateCommand OpenSystemMenuCommand { get; protected set; }

        public DelegateCommand LoadEapFileCommand { get; protected set; }

        public AsyncCommand RefreshCommand { get; protected set; }

        public DelegateCommand ReauthenticateCommand { get; protected set; }

        public DelegateCommand RemoveProfileCommand { get; protected set; }

        public DelegateCommand RemoveCertificatesCommand { get; protected set; }

        public DelegateCommand UninstallCommand { get; protected set; }

        public DelegateCommand OpenHelpCommand { get; protected set; }

        public DelegateCommand CancelUpdateCommand { get; protected set; }
        public DelegateCommand OpenUpdateMenuCommand { get; protected set; }
        public DelegateCommand DoUpdateCommand { get; protected set; }
        public DelegateCommand OpenBrowserCommand { get; protected set; }
        public DelegateCommand CopyLinkCommand { get; protected set; }
        public DelegateCommand NotificationCommand { get; protected set; }

        public Action CloseApp { get; set; }

        private bool _isLoading { get; set; }
        public bool IsLoading {
            get
            {
                return (!IsConnected && _isLoading) ? false : _isLoading;
            }
            private set
            {
                _isLoading = value;
            }
        }

        public bool ShowSystemMenu { get; set; }
        public bool ShowUpdateMenu { get; set; }

        public bool ShowLogo
        {
            get
            {
                if (this.ActiveContent == null)
                {
                    return true;
                }
                else
                {
                    return this.ActiveContent.ShowLogo;
                }
            }
        }

        public string AppVersion
        {
            get
            {
                this.status = new StatusTask().GetStatus();

                return this.status.Version;
            }
        }

        public string AppTitle => Settings.Settings.ApplicationName;

        public string PageTitle
        {
            get
            {
                if (this.ActiveContent == null)
                {
                    return string.Empty;
                }
                return this.ActiveContent.PageTitle;
            }
        }

        public bool ShowNavigatePrevious
        {
            get
            {
                if (this.ActiveContent == null)
                {
                    return false;
                }
                return this.ActiveContent.ShowNavigatePrevious;
            }
        }

        public bool ShowNavigateNext
        {
            get
            {
                if (this.ActiveContent == null)
                {
                    return false;
                }
                return this.ActiveContent.ShowNavigateNext;
            }
        }

        public void OnConfirmUpdate()
        {
            Task.Run(async () =>
            {
                await UpdateChecker.DownloadUpdateAsync();
            });
        }

        public void OnOpenBrowser()
        {
            Process.Start(Settings.Settings.BrowserDownloadUrl);
        }
        public void OnCopyLink()
        {
            Clipboard.SetText(Settings.Settings.BrowserDownloadUrl);
        }
        public void VersionCheck()
        {
            this.SelfTestSuccess = true;
            this.NotificationDismiss = true;
            if(!ArchitectureHelper.ProcessIsNative())
            {
                // Show a notification bar with a button to open the browser
                this.SelfTestSuccess = false;
                this.NotificationDismiss = false;
                this.ShowNotificationBar = true;
                this.NotificationButtonCaption = Resources.OpenBrowserButton;
                this.NotificationCommand = this.OpenBrowserCommand;
                this.NotificationText = string.Format(Resources.IncompatibleVersionMessage, Settings.Settings.ApplicationName);

                if (UpdateChecker.NewVersion == null || SemVersion.ComparePrecedence(SelfInstaller.DefaultInstance.GetRunningVersion(), UpdateChecker.NewVersion) == 1)
                {
                    // Our version is newer than what's available, or nothing is available
                    this.NotificationButtonCaption = Resources.OpenBrowserButton;
                    this.NotificationCommand = this.OpenBrowserCommand;
                }
                else
                {
                    // There is an update available, or the same version is available
                    this.NotificationButtonCaption = Resources.UpdateNowButton;
                    this.NotificationCommand = this.DoUpdateCommand;
                    this.UpdateAvailable = true;
                }
            }

            if (UpdateChecker.IsUpdateAvailable)
            {
                if (!ArchitectureHelper.ProcessIsNative())
                {
                    this.NotificationText = string.Format(Resources.IncompatibleVersionMessage, Settings.Settings.ApplicationName)
                        + "\n" + string.Format(Resources.UpdateAvailableWithVersionNo, Settings.Settings.ApplicationName, UpdateChecker.NewVersion);
                }
                else
                {
                    this.NotificationText = string.Format(Resources.UpdateAvailableWithVersionNo, Settings.Settings.ApplicationName, UpdateChecker.NewVersion);
                }
                this.ShowNotificationBar = true;
                this.UpdateAvailable = true;

                this.NotificationButtonCaption = Resources.UpdateNowButton;
                this.NotificationCommand = this.DoUpdateCommand;

                if (SemVersion.ComparePrecedence(SelfInstaller.DefaultInstance.GetRunningVersion(), UpdateChecker.MinimalSupportedVersion) == -1)
                {
                    // This version is older than our minimal supported version
                    this.NotificationText = string.Format(Resources.VersionNoLongerSupported, Settings.Settings.ApplicationName, SelfInstaller.GetRunningVersion(), UpdateChecker.NewVersion);
                    this.SelfTestSuccess = false;
                    this.NotificationDismiss = false;
                }
            }

            if (SemVersion.ComparePrecedence(SelfInstaller.DefaultInstance.GetRunningVersion(), UpdateChecker.NewVersion) == 1)
            {
                // We are running a newer version than available; we cannot automatically download the correct version
                // Replace the download button with a download button
                this.UpdateAvailable = false;
                this.NotificationButtonCaption = Resources.OpenBrowserButton;
                this.NotificationCommand = this.OpenBrowserCommand;
            }

            this.CallPropertyChanged(nameof(this.SelfTestSuccess));
            this.CallPropertyChanged(nameof(this.NotificationText));
            this.CallPropertyChanged(nameof(this.UpdateAvailable));
            this.CallPropertyChanged(nameof(this.ShowNotificationBar));
            this.CallPropertyChanged(nameof(this.NotificationButtonCaption));
            this.CallPropertyChanged(nameof(this.NotificationCommand));
        }

        public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EapConfigTask.IsEapConfigSupported(eapConfig))
            {
                _ = MessageBox.Show(
                    EduRoam.Localization.Resources.WarningProfileNotSupported,
                    EduRoam.Localization.Resources.WarningNoSupportedAuthenticationMethod,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void SetPreviousActiveContent()
        {
            if (!this.State.NavigationHistory.Any())
            {
                return;
            }

            var viewModel = this.State.NavigationHistory.Pop();

            this.ActiveContent = viewModel;
            this.IsLoading = false;
            this.CallViewPropertyChanges();
        }

        public void SetActiveContent(BaseViewModel viewModel)
        {
            this.IsLoading = true;

            Task.Run(
                () =>
                {
                    if (this.ActiveContent != null)
                    {
                        this.State.NavigationHistory.Push(this.ActiveContent);
                    }

                    this.ActiveContent = viewModel;
                    this.IsLoading = false;
                    this.CallViewPropertyChanges();
                });
        }

        private void CallViewPropertyChanges()
        {
            this.CallPropertyChanged(nameof(this.ActiveContent));
            this.CallPropertyChanged(nameof(this.ShowNavigatePrevious));
            this.CallPropertyChanged(nameof(this.ShowNavigateNext));
            this.CallPropertyChanged(nameof(this.ShowLogo));
            this.CallPropertyChanged(nameof(this.PageTitle));
        }

        private void CallProfilePropertyChanges()
        {
            this.CallPropertyChanged(nameof(this.CanProfileBeRemoved));
            this.CallPropertyChanged(nameof(this.CanCertificatesBeRemoved));
            this.CallPropertyChanged(nameof(this.IsARefreshPossible));
            this.CallPropertyChanged(nameof(this.IsReauthenticatePossible));
        }

        public void SelectInstitution()
        {
            this.State.Reset();
            this.SetActiveContent(new SelectInstitutionViewModel(this));
        }
        
        /// <summary>
        /// downloads eap config based on profileId
        /// seperated into its own function as this can happen either through
        /// user selecting a profile or a profile being autoselected
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="eapConfigXml"></param>
        /// <param name="skipOverview"></param>
        /// <returns>True if function navigated somewhere</returns>
        /// <exception cref="XmlException">Parsing eap-config failed</exception>
        public async Task HandleProfileSelect(
            string profileId,
            IdentityProviderProfile? profile = null,
            string? eapConfigXml = null,
            bool skipOverview = false)
        {
            EapConfig? eapConfig;

            this.IsLoading = true;

            try
            {
                if (profile == null)
                {
                    profile = await IdentityProviderDownloader.Instance.GetProfileFromId(profileId);

                    if (profile == null)
                    {
                        this.Logger.LogError($"Unknown Profile, profile with id {profileId} could not be found.");
                        MessageBox.Show(EduRoam.Localization.Resources.ErrorUnknownProfile, caption: $"{Settings.Settings.ApplicationName} - Exception");
                        return;
                    }
                }             

                this.State.SelectedProfile = profile;
            }
            catch (EduroamAppUserException eauExc)
            {
                this.Logger.LogError(eauExc, $"{Settings.Settings.ApplicationName} - Exception");
                MessageBox.Show(eauExc.UserFacingMessage, caption: $"{Settings.Settings.ApplicationName} - Exception");
                return;
            }

            if (!string.IsNullOrWhiteSpace(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                this.Logger.LogInformation($"category: {nameof(this.HandleProfileSelect)}, {nameof(eapConfigXml)} was set");

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }
            else
            {
                this.Logger.LogInformation($"category: {nameof(this.HandleProfileSelect)}, {nameof(eapConfigXml)} was not set");
            }

            if (profile.OAuth)
            {
                this.SetActiveContent(new OAuthViewModel(this));
            }
            else if (!string.IsNullOrWhiteSpace(profile.Redirect))
            {
                this.SetActiveContent(new RedirectViewModel(this, new Uri(profile.Redirect)));
            }
            else
            {
                var eapConfiguration = new EapConfigTask();

                eapConfig = await eapConfiguration.GetEapConfigAsync(profile);
                if (eapConfig != null)
                {
                    if (eapConfig.HasInfo && !skipOverview)
                    {
                        this.SetActiveContent(new ProfileViewModel(this, eapConfig));
                    }
                    else
                    {
                        var configureTask = new ConfigureTask(eapConfig);
                        var installers = configureTask.GetCertificateInstallers();

                        if (installers.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                        {
                            this.SetActiveContent(new CertificateViewModel(this, eapConfig));
                        }
                        else
                        {
                            this.Connect(eapConfig);
                        }
                    }

                }
            }
        }

        public void Connect(EapConfig eapConfig)
        {
            // Connect
            var configure = new ConfigureTask(eapConfig);
            var connector = configure.GetConnector();

            switch (connector)
            {
                case CredentialsConnector credentialsConnector:
                    this.SetActiveContent(new ConnectWithCredentialsViewModel(this, eapConfig, credentialsConnector));
                    break;
                case CertPassConnector certPassConnector:
                    this.SetActiveContent(new ConnectWithCertificatePassphraseViewModel(this, eapConfig, certPassConnector));
                    break;
                case CertAndCertPassConnector certAndCertPassConnector:
                    this.SetActiveContent(new ConnectWithLocalCertificatePassphraseViewModel(this, eapConfig, certAndCertPassConnector));
                    break;
                case DefaultConnector defaultConnector:
                    this.SetActiveContent(new ConnectViewModel(this, eapConfig, defaultConnector));
                    break;
                default:
                    throw new NotSupportedException(string.Format(EduRoam.Localization.Resources.ErrorUnsupportedConnectionType, connector?.GetType().Name));

            }
        }

        public bool CanEapFileBeLoaded => true;

        /// <summary>
		/// Asks the user to supply a .eap-config file.
		/// Returns null if user aborted.
		/// </summary>
		/// <returns></returns>
		public void GetEapFileFromDialog()
        {
            Debug.WriteLine("LoadEapFile");

            string? filepath;
            do
            {
                filepath = FileDialog.GetFileFromDialog(
                    EduRoam.Localization.Resources.LoadEapFile,
                    "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*");

                if (filepath == null)
                {
                    return; // the user canelled
                }
            }
            while (!FileDialog.ValidateFile(filepath, new List<string> { ".eap-config" }));

            this.LoadEapFile(filepath);
        }

        public bool IsARefreshPossible {
            get => ArchitectureHelper.ProcessIsNative() && this.status.ActiveProfile;
        }

        public async Task RefreshAsync()
        {
            if (this.status.ActiveProfile)
            {
                await RefreshTask.RefreshAsync(true);
            }
        }

        public bool IsReauthenticatePossible
        {
            get => ArchitectureHelper.ProcessIsNative() && this.status.ActiveProfile;
        }

        public void Reauthenticate()
        {
            if (this.status.ActiveProfile)
            {
                var profileId = this.status.Identity.Value.ProfileId!;

                Task.Run(() => this.HandleProfileSelect(profileId));
            }
        }

        public bool CanProfileBeRemoved => this.status.ActiveProfile;

        public void RemoveProfile()
        {
            if (this.status.ActiveProfile)
            {
                var profiler = new ProfilesTask();
                var profileName = profiler.GetCurrentProfileName();

                var confirmRemoval = MessageBox.Show(
                        string.Format(EduRoam.Localization.Resources.RemoveProfileMessage, profileName),
                        string.Format(EduRoam.Localization.Resources.RemoveProfileTitle, profileName),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                if (confirmRemoval == MessageBoxResult.OK)
                {
                    profiler.RemoveCurrentProfile();

                    // Reset the state and set the content to the status view model
                    this.State.Reset();
                    this.status = new StatusTask().GetStatus();
                    this.CallProfilePropertyChanges();
                    this.SetActiveContent(new StatusViewModel(this));
                }
            }
        }

        public bool CanCertificatesBeRemoved => this.status.ActiveProfile;

        public void RemoveCertificates()
        {
            RemoveWiFiConfigurationTask.RemoveCertificates(false);
        }

        public bool CanAppBeUninstalled => UninstallTask.AppIsInstalled;

        public ILogger<MainViewModel> Logger { get; }

        public void Uninstall()
        {
            var result = MessageBox.Show(
                string.Format(EduRoam.Localization.Resources.WarningUninstall, Settings.Settings.ApplicationName), 
                EduRoam.Localization.Resources.CommandDescriptionUninstall, 
                MessageBoxButton.OKCancel
            );
            if (result == MessageBoxResult.OK) try
            {
                UninstallTask.Uninstall(true);
            }
            finally
            {
                this.CloseApp();
            }
        }

        public void OpenSystemMenu()
        {
            this.ShowSystemMenu = true;
            this.CallPropertyChanged(nameof(this.ShowSystemMenu));
        }

        public void OpenHelp()
        {
            var helpUrl = Settings.Settings.HelpUrl;

            if (!string.IsNullOrWhiteSpace(helpUrl))
            {
                Process.Start(new ProcessStartInfo(helpUrl) { UseShellExecute = true });
            }
        }

        public void DismissNotification()
        {
            this.ShowNotificationBar = false;
            this.CallPropertyChanged(nameof(this.ShowNotificationBar));
        }
        public void OpenUpdateMenu()
        {
            this.ShowUpdateMenu = true;
            this.CallPropertyChanged(nameof(this.UpdateAvailable));
            this.CallPropertyChanged(nameof(this.ShowUpdateMenu));
        }
        private void LoadEapFile(string filepath)
        {
            // read, validate, parse and return
            try
            {
                var eapConfigurator = new EapConfigTask();
                // create Eap-config and open Profile view
                var eapConfig = EapConfigTask.GetEapConfig(new FileInfo(filepath));

                if (eapConfig != null)
                {
                    eapConfig.ProfileId = filepath;

                    this.SetActiveContent(new ProfileViewModel(this, eapConfig));
                }
            }
            catch (System.Xml.XmlException xmlEx)
            {
                MessageBox.Show(
                    EduRoam.Localization.Resources.ErrorEapConfigCorrupted +
                    "\nException: " + xmlEx.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(
                    EduRoam.Localization.Resources.ErrorEapConfigInvalid +
                    "\nException: " + argEx.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
#pragma warning restore CA1822 // Mark members as static
}