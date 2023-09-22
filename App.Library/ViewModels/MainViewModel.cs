﻿using App.Library.Command;
using App.Library.Utility;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace App.Library.ViewModels
{
#pragma warning disable CA1822 // Members are bound by a template and therefore cannot be static
    public class MainViewModel : NotifyPropertyChanged, IDisposable
    {
        private readonly IdentityProviderDownloader idpDownloader;

        public static readonly SelfInstaller SelfInstaller = SelfInstaller.DefaultInstance;

        private readonly Status status;

        public MainViewModel(ILogger<MainViewModel> logger)
        {
            this.NewProfileCommand = new DelegateCommand(this.NewProfileCommandAction, this.CanNewProfileCommandAction);
            this.LoadEapFileCommand = new DelegateCommand(this.LoadEapFile);
            this.RefreshCommand = new AsyncCommand(this.RefreshAsync);
            this.ReauthenticateCommand = new DelegateCommand(this.Reauthenticate);
            this.RemoveProfileCommand = new DelegateCommand(this.RemoveProfile);
            this.RemoveCertificatesCommand = new DelegateCommand(this.RemoveCertificates);
            this.UninstallCommand = new DelegateCommand(this.Uninstall);
            this.OpenHelpCommand = new DelegateCommand(this.OpenHelp);
            this.OpenMenuCommand = new DelegateCommand(this.OpenMenu);

            this.idpDownloader = new IdentityProviderDownloader();
            this.State = new ApplicationState();

            this.status = new StatusTask().GetStatus();
            this.IsLoading = true;
            this.Logger = logger;

            this.Logger.LogInformation($"{this.AppTitle}, version: {this.AppVersion}, run as admin: {StatusTask.RunAsAdministrator}");

            Task.Run(
                async () =>
                {
                    await this.idpDownloader.LoadProviders();
                    this.IsLoading = false;

                    this.SetStartContent();
                    this.CallPropertyChanged(string.Empty);
                    DelegateCommand.RaiseCanExecuteChanged();
                });

        }

        public ApplicationState State { get; private set; }

        public BaseViewModel? ActiveContent { get; private set; }

        public DelegateCommand NewProfileCommand { get; protected set; }

        public DelegateCommand OpenMenuCommand { get; protected set; }

        public DelegateCommand LoadEapFileCommand { get; protected set; }

        public AsyncCommand RefreshCommand { get; protected set; }

        public DelegateCommand ReauthenticateCommand { get; protected set; }

        public DelegateCommand RemoveProfileCommand { get; protected set; }

        public DelegateCommand RemoveCertificatesCommand { get; protected set; }

        public DelegateCommand UninstallCommand { get; protected set; }

        public DelegateCommand OpenHelpCommand { get; protected set; }

        public Action CloseApp { get; set; }

        public bool IsLoading { get; private set; }

        public bool ShowMenu { get; set; }

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
                var statusTask = new StatusTask();
                var status = statusTask.GetStatus();

                return status.Version;
            }
        }

        public string AppTitle
        {
            get
            {
                var appAssemblyName = Assembly.GetEntryAssembly()!.GetName();

                if (!string.IsNullOrWhiteSpace(appAssemblyName.CultureName))
                {
                    return appAssemblyName.CultureName;
                }
                return appAssemblyName.Name ?? "geteduroam";
            }
        }

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

        public void SetStartContent()
        {
            var status = new StatusTask().GetStatus();

            if (status.ActiveProfile)
            {
                this.SetActiveContent(new StatusViewModel(this));
            }
            else
            {
                var eapConfig = EapConfigTask.GetBundledEapConfig();

                if (eapConfig != null)
                {

                    this.SetActiveContent(new ProfileViewModel(this, eapConfig));
                }
                else
                {
                    this.SetActiveContent(new StatusViewModel(this));
                }
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

        public void Restart()
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
            catch (EduroamAppUserException eauExc)
            {
                this.Logger.LogError(eauExc, "geteduroam - Exception");
                MessageBox.Show(eauExc.UserFacingMessage, caption: "geteduroam - Exception");
                return;
            }

            if (!string.IsNullOrWhiteSpace(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                this.Logger.LogInformation($"category: {nameof(this.HandleProfileSelect)}, {nameof(eapConfigXml)} was set");

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }

            this.Logger.LogInformation($"category: {nameof(this.HandleProfileSelect)}, {nameof(eapConfigXml)} was not set");

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

        public bool CanEapFileBeLoaded => true;

        /// <summary>
		/// Asks the user to supply a .eap-config file.
		/// Returns null if user aborted.
		/// </summary>
		/// <returns>EapConfig object or null</returns>
		/// <exception cref="XmlException"></exception>
		public void LoadEapFile()
        {
            Debug.WriteLine("LoadEapFile");

            string? filepath;
            do
            {
                filepath = FileDialog.GetFileFromDialog(
                    Resources.LoadEapFile,
                    "EAP-CONFIG files (*.eap-config)|*.eap-config|All files (*.*)|*.*");

                if (filepath == null)
                {
                    return; // the user canelled
                }
            }
            while (!FileDialog.ValidateFile(filepath, new List<string> { ".eap-config" }));

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

        public bool IsARefreshPossible => this.status.ActiveProfile;

        public async Task RefreshAsync()
        {
            if (this.status.ActiveProfile)
            {
                await RefreshTask.RefreshAsync(true);
            }
        }

        public bool IsReauthenticatePossible => this.status.ActiveProfile;

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
                        string.Format(Resources.RemoveProfileMessage, profileName),
                        string.Format(Resources.RemoveProfileTitle, profileName),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                if (confirmRemoval == MessageBoxResult.OK)
                {
                    profiler.RemoveCurrentProfile();

                    this.Restart();
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
            UninstallTask.Uninstall(_ => this.CloseApp());
        }

        public void OpenMenu()
        {
            this.ShowMenu = true;
            this.CallPropertyChanged(nameof(this.ShowMenu));
        }

        public void OpenHelp()
        {
            var helpUrl = ApplicationResources.GetString("HelpUrl");

            if (!string.IsNullOrWhiteSpace(helpUrl))
            {
                Process.Start(new ProcessStartInfo(helpUrl) { UseShellExecute = true });
            }
        }
    }
#pragma warning restore CA1822 // Mark members as static
}