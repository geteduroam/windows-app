using App.Library.Command;
using App.Library.Utility;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace App.Library.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged, IDisposable
    {
        private readonly IdentityProviderDownloader idpDownloader;

        public static readonly SelfInstaller SelfInstaller = SelfInstaller.DefaultInstance;

        public MainViewModel()
        {
            this.NewProfileCommand = new DelegateCommand(this.NewProfileCommandAction, this.CanNewProfileCommandAction);
            this.idpDownloader = new IdentityProviderDownloader();
            this.State = new ApplicationState();

            this.IsLoading = true;

            Task.Run(
                async () =>
                {
                    await this.idpDownloader.LoadProviders(useGeodata: true);
                    this.IsLoading = false;

                    this.SetActiveContent(new StatusViewModel(this));
                    this.CallPropertyChanged(string.Empty);
                    this.NewProfileCommand.RaiseCanExecuteChanged();
                });
        }

        public ApplicationState State { get; private set; }

        public BaseViewModel? ActiveContent { get; private set; }

        public DelegateCommand NewProfileCommand { get; protected set; }

        public bool IsLoading { get; private set; }

#pragma warning disable CA1822 // Mark members as static
        public string AppVersion
#pragma warning restore CA1822 // Mark members as static
        {
            get
            {
                var statusTask = new StatusTask();
                var status = statusTask.GetStatus();

                return status.Version;
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

        public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EapConfigTask.IsEapConfigSupported(eapConfig))
            {
                _ = MessageBox.Show(
                    Resources.WarningProfileNotSupported,
                    Resources.WarningNoSupportedAuthenticationMethod,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        private bool CanNewProfileCommandAction()
        {
            return this.idpDownloader.Loaded;
        }

        private void NewProfileCommandAction()
        {
            this.SetActiveContent(new SelectInstitutionViewModel(this));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.idpDownloader.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.idpDownloader.Dispose();
            }
        }

        public void SetPreviousActiveContent()
        {
            if (this.State.NavigationHistory.TryPop(out var viewModel))
            {
                this.ActiveContent = viewModel;
                this.IsLoading = false;
                this.CallPropertyChanged(nameof(this.ActiveContent));
            }
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
                    this.CallPropertyChanged(nameof(this.ActiveContent));
                    this.CallPropertyChanged(nameof(this.IsLoading));
                    this.CallPropertyChanged(nameof(this.ShowNavigatePrevious));
                    this.CallPropertyChanged(nameof(this.ShowNavigateNext));
                });
        }

        public void Restart()
        {
            this.State.Reset();
            this.SetActiveContent(new SelectInstitutionViewModel(this));
        }

        //todo Move to a better place

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
            string? eapConfigXml = null,
            bool skipOverview = false)
        {
            IdentityProviderProfile? profile;
            EapConfig? eapConfig;

            this.IsLoading = true;

            try
            {
                profile = this.idpDownloader.GetProfileFromId(profileId);

                if (profile == null)
                {
                    MessageBox.Show(Resources.ErrorUnknownProfile, caption: "geteduroam - Exception");
                    return;
                }

                this.State.SelectedProfile = profile;
            }
            catch (EduroamAppUserException ex) // TODO: catch this on some higher level
            {
                MessageBox.Show(ex.UserFacingMessage, caption: "geteduroam - Exception");
                return;
            }

            if (!string.IsNullOrWhiteSpace(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(this.HandleProfileSelect));

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }

            Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(this.HandleProfileSelect));

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

                eapConfig = await eapConfiguration.GetEapConfigAsync(profile.Id);
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
                    throw new NotSupportedException(string.Format(Resources.ErrorUnsupportedConnectionType, connector?.GetType().Name));

            }
        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        /// <exception cref="EduroamAppUserException">description</exception>
        private async Task<EapConfig?> DownloadEapConfig(IdentityProviderProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile?.Id))
            {
                return null;
            }

            // if OAuth
            if (profile.OAuth
                || !string.IsNullOrEmpty(profile.Redirect))
            {
                return null;
            }

            try
            {
                return await Task.Run(() => this.idpDownloader.DownloadEapConfig(profile.Id));
            }
            catch (ApiUnreachableException e)
            {
                throw new EduroamAppUserException(
                    "HttpRequestException",
                    "Couldn't connect to the server.\n\n"
                    + "Make sure that you are connected to the internet, then try again.\n"
                    + "Exception: "
                    + e.Message);
            }
            catch (ApiParsingException e)
            {
                throw new EduroamAppUserException(
                    "xml parse exception",
                    "The institution or profile is either not supported or malformed. "
                    + "Please select a different institution or profile.\n\n"
                    + "Exception: "
                    + e.Message);
            }
        }

        /// <summary>
		/// Asks the user to supply a .eap-config file.
		/// Returns null if user aborted.
		/// </summary>
		/// <returns>EapConfig object or null</returns>
		/// <exception cref="XmlException"></exception>
		public async Task LoadEapFile()
        {
            Debug.WriteLine("LoadEapFile");

            string? filepath;
            do
            {
                filepath = FileDialog.GetFileFromDialog(
                    Resources.LoadEapFile,
                    "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*");

                if (filepath == null) return; // the user canelled
            }
            while (!FileDialog.ValidateFile(filepath, new List<string> { ".eap-config" }));

            // read, validate, parse and return
            try
            {
                var eapConfigurator = new EapConfigTask();
                // create Eap-config and open Profile view
                var eapConfig = await EapConfigTask.GetEapConfigAsync(new FileInfo(filepath));

                if (eapConfig != null)
                {
                    eapConfig.ProfileId = filepath;

                    this.SetActiveContent(new ProfileViewModel(this, eapConfig));
                }
            }
            catch (System.Xml.XmlException xmlEx)
            {
                MessageBox.Show(
                    Resources.ErrorEapConfigCorrupted +
                    "\nException: " + xmlEx.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(
                    Resources.ErrorEapConfigInvalid +
                    "\nException: " + argEx.Message,
                    "eduroam - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Refresh()
        {
            Task.Run(() => RefreshTask.RefreshAsync(true));
        }

        public void Reauthenticate()
        {
            throw new NotImplementedException();
        }

        public void RemoveProfile()
        {
            var profiler = new ProfilesTask();
            var profileName = profiler.GetCurrentProfileName();
            if (MessageBoxResult.OK == MessageBox.Show(
                    "This will remove all configuration for\r\n" + profileName,
                    "Remove " + profileName,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                    ))
            {
                var remover = new RemoveWiFiConfigurationTask();
                remover.Remove(omitRootCa: true);

                this.Restart();
            }
        }

        public void RemoveCertificates()
        {
            throw new NotImplementedException();
        }

        public void Uninstall(Action<bool> afterUninstall)
        {
            var uninstaller = new UninstallTask();
            uninstaller.Uninstall(afterUninstall);
        }
    }
}