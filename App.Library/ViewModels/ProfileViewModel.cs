using App.Library.Command;

using EduRoam.Connect.Eap;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly EapConfig eapConfig;

        public ProfileViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
            this.eapConfig = eapConfig;
            this.NavigateWebCommand = new DelegateCommand(this.NavigateWeb, this.CanNavigateWeb);
            this.OpenEmailCommand = new DelegateCommand(this.OpenEmail, this.CanOpenEmail);
            this.SelectOtherInstitutionCommand = new DelegateCommand(this.SelectOtherInstitution, () => true);
            this.ShowTermsOfUseCommand = new DelegateCommand(this.ShowTermsOfUse, () => true);

            //todo ExtractFlag?
            //todo CopyToClipboard WebAddress / Phone / Email?
        }

        public DelegateCommand NavigateWebCommand { get; private set; }

        public DelegateCommand OpenEmailCommand { get; private set; }

        public DelegateCommand SelectOtherInstitutionCommand { get; private set; }

        public DelegateCommand ShowTermsOfUseCommand { get; private set; }

        public string Name => this.eapConfig.InstitutionInfo.DisplayName;

        public string Description => this.eapConfig.InstitutionInfo.Description;

        public bool HasContactInfo
        {
            get
            {
                var hasWebAddress = !string.IsNullOrEmpty(this.eapConfig.InstitutionInfo.WebAddress);
                var hasEmailAddress = !string.IsNullOrEmpty(this.eapConfig.InstitutionInfo.EmailAddress);
                var hasPhone = !string.IsNullOrEmpty(this.eapConfig.InstitutionInfo.Phone);
                return (hasWebAddress || hasEmailAddress || hasPhone);
            }
        }

        public string WebAddress => this.eapConfig.InstitutionInfo.WebAddress;

        public bool WebAddressIsValid =>
            Uri.TryCreate(this.WebAddress, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        public string EmailAddress => this.eapConfig.InstitutionInfo.EmailAddress;

        public bool EmailIsValid => !(this.EmailAddress.Contains(' ') || !this.EmailAddress.Contains('@'));

        public string Phone => this.eapConfig.InstitutionInfo.Phone;

        public bool PhoneIsValid => !string.IsNullOrEmpty(this.Phone);

        public string TermsOfUse => this.eapConfig.InstitutionInfo.TermsOfUse;

        public bool HasTermsOfUse => !string.IsNullOrEmpty(this.TermsOfUse);

        private bool CanNavigateWeb()
        {
            return this.WebAddressIsValid;
        }

        private void NavigateWeb()
        {
            var openWebPageCommand = new Uri(this.WebAddress).ToString();
            Process.Start(new ProcessStartInfo(openWebPageCommand) { UseShellExecute = true });
        }

        private bool CanOpenEmail()
        {
            return this.EmailIsValid;
        }

        private void OpenEmail()
        {
            var sendMailCommand = new Uri("mailto:" + this.EmailAddress).ToString();
            Process.Start(new ProcessStartInfo(sendMailCommand) { UseShellExecute = true });
        }

        private void SelectOtherInstitution()
        {
            this.Owner.Restart();
        }

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            //todo ShowTou was always true in old situation, What to do?

            //if (pageProfileOverview.ShowTou)
            //{
            //    LoadPageTermsOfUse();
            //    break;
            //}
            //if (ConnectToEduroam.EnumerateCAInstallers(eapConfig)
            //.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
            //{
            //    LoadPageCertificateOverview();
            //    break;
            //}

            //LoadPageLogin();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Loads the logo form the curent eapconfig if it exists. Else display Eduroam logo.
        /// </summary>
        private void LoadProviderLogo()
        {
            //todo svg support imageConverter,
            //todo seperate image not override eduram logo

            //byte[] logoBytes = eapConfig.InstitutionInfo.LogoData;
            //string logoMimeType = eapConfig.InstitutionInfo.LogoMimeType;
        }

        private void ShowTermsOfUse()
        {
            this.Owner.SetActiveContent(new TermsOfUseViewModel(this.Owner, this.eapConfig));
        }
    }
}