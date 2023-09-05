using App.Library.Binding;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Tasks;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class SelectInstitutionViewModel : BaseViewModel
    {
        private string searchText;

        public SelectInstitutionViewModel(MainViewModel owner)
            : base(owner)
        {
            this.searchText = string.Empty;
        }

        public override bool ShowSearch => true;

        public override void Search(string query)
        {
            this.searchText = query;
            this.CallPropertyChanged(nameof(this.Institutions));
        }

        public AsyncProperty<ObservableCollection<IdentityProvider>> Institutions
        {
            get
            {
                return new AsyncProperty<ObservableCollection<IdentityProvider>>(this.GetInstitutionsAsync());
            }
        }

        public async Task<ObservableCollection<IdentityProvider>> GetInstitutionsAsync()
        {
            var institutes = await InstitutesTask.GetAsync(this.searchText);
            return new ObservableCollection<IdentityProvider>(institutes);
        }

        protected override bool CanNavigateNextAsync()
        {
            return this.Owner.State.SelectedIdentityProvider != null;
        }

        protected override async Task NavigateNextAsync()
        {
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
                    await this.Owner.HandleProfileSelect(autoProfile.Id);
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