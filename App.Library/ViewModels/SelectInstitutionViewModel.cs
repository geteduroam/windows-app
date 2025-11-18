using App.Library.Binding;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace App.Library.ViewModels
{
    public class SelectInstitutionViewModel : BaseViewModel
    {
        public SelectInstitutionViewModel(MainViewModel owner)
            : base(owner)
        {
            this.preload();
        }

        private async void preload()
        {
            this.CallPropertyChanged(nameof(this.Activated));
            this.CallPropertyChanged(nameof(this.Disconnected));

            try
            {
                await IdentityProviderDownloader.Instance.LoadProviders();
            }
            catch (Exception)
            {
                Debug.WriteLine("Preloading discovery failed. Will retry on user interaction. This may happen when the internet connection is down.");
            }

            Activated = true;
            this.CallPropertyChanged(nameof(this.Loaded));
            this.CallPropertyChanged(nameof(this.Searching));
            this.CallPropertyChanged(nameof(this.Disconnected));
        }

        public string WaitingConnectionText {
            get => string.Format(SharedResources.NoConnection, Settings.Settings.ApplicationName); 
        }

        private string searchText = string.Empty;
        public string SearchText
        {
            get => this.searchText;
            set
            {
                this.searchText = value;
                this.CallPropertyChanged(nameof(this.Institutions));
            }
        }
        public bool Loaded => IdentityProviderDownloader.Instance.Loaded;
        public bool Disconnected { get => this.Activated && !this.Loaded; }
        public bool Searching { get => this.Loaded && !string.IsNullOrWhiteSpace(this.SearchText); }
        public bool Activated { private set; get; }
        public override string PageTitle => SharedResources.SelectInstitution;

        public AsyncProperty<ObservableCollection<IdentityProvider>> Institutions
        {
            get
            {
                return new AsyncProperty<ObservableCollection<IdentityProvider>>(this.PerformSearchAsync());
            }
        }

        public async Task<ObservableCollection<IdentityProvider>> PerformSearchAsync()
        {
            var institutes = await InstitutesTask.SearchAsync(this.searchText.Trim());
            var urlProvider = GetUrlProvider(this.searchText.Trim());
            if (urlProvider != null)
            {
                institutes = institutes.Prepend(urlProvider);
            }

            // In case Loaded has changed, we retry the institution list on each keypress,
            // as long as we haven't succeeded
            this.CallPropertyChanged(nameof(this.Loaded));
            this.CallPropertyChanged(nameof(this.Disconnected));

            this.CallPropertyChanged(nameof(this.Searching));
            return new ObservableCollection<IdentityProvider>(institutes);
        }

        private static IdentityProvider GetUrlProvider(string url)
        {
            url = url.Trim('.');
            if (!url.StartsWith("https://") && !url.StartsWith("http://") && url.Count(c => c == '.') >= 1)
                url = $"https://{url}";
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.PathAndQuery == "/" && (uri.Scheme == "https" || uri.Scheme == "http"))
            {
                return new IdentityProvider
                {
                    Id = "custom_http_provider",
                    Name = string.Format(SharedResources.ConnectTo0, url),
                    DownloadMetadataOnSelect = true
                };
            }

            return null;
        }

        protected override bool CanNavigateNextAsync()
        {
            return this.Owner.State.SelectedIdentityProvider != null || GetUrlProvider(this.searchText.Trim()) != null;
        }

        protected override async Task NavigateNextAsync()
        {
            var provider = this.Owner.State.SelectedIdentityProvider ?? GetUrlProvider(this.searchText.Trim());
            if(provider.DownloadMetadataOnSelect)
            {
                try
                {
                    this.Owner.State.SelectedIdentityProvider = await InstitutesTask.GetProfileFromUrlAsync(this.searchText);
                } catch (EduroamAppUserException ex)
                {
                    this.Owner.SetActiveContent(new ConfirmViewModel(this.Owner, string.Format("{0}{1}", ex.UserFacingMessage, string.IsNullOrEmpty(ex.Message) ? "" : $": {ex.Message}"), () => { this.Owner.SetActiveContent(this); }));
                    this.Owner.Logger.LogError(string.Format("{0}{1}", ex.UserFacingMessage, string.IsNullOrEmpty(ex.Message) ? "" : $": {ex.Message}"));
                    return;
                }
            }

            var availableProfiles = this.Owner.State.SelectedIdentityProvider?.Profiles.Count ?? 0;

            if (availableProfiles == 0)
            {
                throw new NotSupportedException("No profiles available for the selected institute");
            }
            else if (availableProfiles == 1) // skip the profile select and go with the first one
            {
                var autoProfile = this.Owner.State.SelectedIdentityProvider!.Profiles.Single();

                if (!string.IsNullOrEmpty(autoProfile.Id))
                {
                    await this.Owner.HandleProfileSelect(autoProfile.Id, autoProfile);
                }
            }
            else
            {
                this.Owner.SetActiveContent(new SelectProfileViewModel(this.Owner));
            }
        }

        /// <summary>
		/// downloads eap config based on profileId
		/// seperated into its own function as this can happen either through
		/// user selecting a profile or a profile being autoselected
		/// </summary>
		/// <param name="profileId"></param>
		/// <param name="eapConfigXml"></param>
		/// <param name="skipOverview"></param>
		/// <returns>True if function navigated somewhere</returns>
		/// <exception cref="XmlException">Parsing eap-config failed</exception>
        /// <exception cref="EduroamAppUserException"/>
		private async Task<bool> HandleProfileSelectAsync(string profileId, string? eapConfigXml, bool skipOverview = false)
        {
            EapConfig? eapConfig = null;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            var profile = await ProfilesTask.GetProfileAsync(profileId);

            if (!string.IsNullOrWhiteSpace(eapConfigXml))
            {
                // TODO: ^perhaps reuse logic from PersistingStore.IsReinstallable
                Debug.WriteLine(nameof(eapConfigXml) + " was set", category: nameof(HandleProfileSelectAsync));

                eapConfig = EapConfig.FromXmlData(eapConfigXml);
                eapConfig.ProfileId = profileId;
            }
            else
            {
                Debug.WriteLine(nameof(eapConfigXml) + " was not set", category: nameof(HandleProfileSelectAsync));

                try
                {
                    var eapConfiguration = new EapConfigTask(new System.Threading.ManualResetEvent(false), new System.Threading.ManualResetEvent(false));
                    eapConfig = await eapConfiguration.GetEapConfigAsync(profileId);
                }
                catch (UnknownProfileException)
                {
                    return false;
                }
            }

            if (eapConfig != null)
            {
                if (!EapConfigTask.IsEapConfigSupported(eapConfig))
                {
                    return false;
                }

                var configure = new ConfigureTask(eapConfig);

                if (eapConfig.HasInfo && !skipOverview)
                {
                    return true;
                }
                if (configure.GetCertificateInstallers()
                        .Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
                {
                    return true;
                }

                return true;
            }
            else if (!string.IsNullOrEmpty(profile?.Redirect))
            {
                return true;
            }
            else if (profile?.OAuth ?? false)
            {
                return true;
            }
            return false;
        }
    }
}