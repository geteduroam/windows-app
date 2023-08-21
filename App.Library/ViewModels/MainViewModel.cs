﻿using App.Library.Command;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Install;
using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using System;
using System.Diagnostics;
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

                    this.SetActiveContent(new SelectInstitutionViewModel(this));
                    this.CallPropertyChanged(string.Empty);
                    this.NewProfileCommand.RaiseCanExecuteChanged();
                });
        }

        public ApplicationState State { get; private set; }

        public BaseViewModel ActiveContent { get; private set; }

        public DelegateCommand NewProfileCommand { get; protected set; }

        public bool IsLoading { get; private set; }

        public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EapConfigTask.IsEapConfigSupported(eapConfig))
            {
                MessageBox.Show(
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
                });
        }

        public void Restart()
        {
            this.State.Reset();
            this.ActiveContent = new SelectInstitutionViewModel(this);
            this.CallPropertyChanged(nameof(this.ActiveContent));
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
        public async Task<bool> HandleProfileSelect(
            IdentityProviderProfile profile,
            string? eapConfigXml = null,
            bool skipOverview = false)
        {
            this.IsLoading = true;
            EapConfig? eapConfig;

            if (!string.IsNullOrEmpty(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(this.HandleProfileSelect));

                eapConfig = null;
                //eapConfig = EapConfig.FromXmlData(eapConfigXml);
                //eapConfig.ProfileId = profile.Id;
            }
            else
            {
                Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(this.HandleProfileSelect));

                profile = this.idpDownloader.GetProfileFromId(profile.Id);
                try
                {
                    eapConfig = await this.DownloadEapConfig(profile);
                }
                catch (EduroamAppUserException ex) // TODO: catch this on some higher level
                {
                    MessageBox.Show(ex.UserFacingMessage, caption: "geteduroam - Exception");
                    eapConfig = null;
                }
            }

            //// reenable buttons after LoadPageLoading() disables them
            //btnBack.IsEnabled = true;
            //btnNext.IsEnabled = true;

            if (eapConfig != null)
            {
                if (!CheckIfEapConfigIsSupported(eapConfig))
                {
                    return false;
                }

                if (eapConfig.HasInfo
                    && !skipOverview)
                {
                    this.SetActiveContent(new ProfileViewModel(this, eapConfig));
                    return true;
                }

                var configureTask = new ConfigureTask(eapConfig);
                var installers = configureTask.GetCertificateInstallers();

                if (installers.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                {
                    this.SetActiveContent(new CertificateViewModel(this, eapConfig));
                    return true;
                }

                this.SetActiveContent(new LoginViewModel(this, eapConfig));
                return true;
            }

            if (!string.IsNullOrEmpty(profile?.Redirect))
            {
                this.SetActiveContent(new RedirectViewModel(this, new Uri(profile.Redirect)));
                return true;
            }

            if (profile?.OAuth ?? false)
            {
                this.SetActiveContent(new OAuthViewModel(this, profile));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets EAP-config file, either directly or after browser authentication.
        /// Prepares for redirect if no EAP-config.
        /// </summary>
        /// <returns>EapConfig object.</returns>
        /// <exception cref="EduroamAppUserException">description</exception>
        public async Task<EapConfig?> DownloadEapConfig(IdentityProviderProfile profile)
        {
            if (string.IsNullOrEmpty(profile?.Id))
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
    }
}