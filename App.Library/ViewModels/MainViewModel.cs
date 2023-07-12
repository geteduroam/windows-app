using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using App.Library.Command;
using App.Library.Language;

using EduRoam.Connect;
using EduRoam.Connect.Exceptions;

namespace App.Library.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged, IDisposable
    {
        private readonly IdentityProviderDownloader idpDownloader;

        public static readonly SelfInstaller SelfInstaller = SelfInstaller.Create();

        public MainViewModel()
        {
            this.LanguageText = new LanguageText(@"App.Library.Language.LanguageTexts.csv", "EN");
            this.NewProfileCommand = new DelegateCommand(this.NewProfileCommandAction, this.CanNewProfileCommandAction);
            this.idpDownloader = new IdentityProviderDownloader();
            this.State = new ApplicationState();

            this.IsLoading = true;

            Task.Run(
                async () =>
                {
                    await this.idpDownloader.LoadProviders(useGeodata: true);
                    this.IsLoading = false;
                    this.CallPropertyChanged(string.Empty);
                    this.NewProfileCommand.RaiseCanExecuteChanged();
                });
        }

        public ApplicationState State { get; private set; }

        public ILanguageText LanguageText { get; }

        public BaseViewModel ActiveContent { get; private set; }

        public DelegateCommand NewProfileCommand { get; protected set; }

        public bool IsLoading { get; private set; }

        public static bool CheckIfEapConfigIsSupported(EapConfig eapConfig)
        {
            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                MessageBox.Show(
                    "The profile you have selected is not supported by this application.",
                    "No supported authentification method was found.",
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
            this.SetActiveContent(new SelectInstitutionViewModel(this, this.idpDownloader));
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

        public void SetActiveContent(BaseViewModel viewModel)
        {
            this.IsLoading = true;

            Task.Run(
                () =>
                {
                    this.ActiveContent = viewModel;
                    this.IsLoading = false;

                    this.CallPropertyChanged(nameof(this.ActiveContent));
                    this.CallPropertyChanged(nameof(this.IsLoading));
                });
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
        private async Task<bool> HandleProfileSelect(
            IdentityProviderProfile profile,
            string eapConfigXml = null,
            bool skipOverview = false)
        {
            this.IsLoading = true;
            EapConfig eapConfig;

            if (!string.IsNullOrEmpty(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(this.HandleProfileSelect));

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profile.Id;
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
                    LoadPageProfileOverview();
                    return true;
                }

                if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
                                    .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                {
                    LoadPageCertificateOverview();
                    return true;
                }

                LoadPageLogin();
                return true;
            }

            if (!string.IsNullOrEmpty(profile?.redirect))
            {
                this.SetActiveContent(new RedirectViewModel(this, new Uri(profile.redirect)));
                return true;
            }
            
            if (profile?.oauth ?? false)
            {
                LoadPageOAuthWait(profile);
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
        public async Task<EapConfig> DownloadEapConfig(IdentityProviderProfile profile)
        {
            if (string.IsNullOrEmpty(profile?.Id))
            {
                return null;
            }

            // if OAuth
            if (profile.oauth
                || !string.IsNullOrEmpty(profile.redirect))
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