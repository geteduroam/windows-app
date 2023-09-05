using App.Library.Command;
using App.Library.Images;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
            //var dynaImage = new BitmapImage(new Uri(@"file://C:\Temp\clock.jpg"));

        }

        public override string PageTitle => this.Name;

        public DelegateCommand NavigateWebCommand { get; private set; }

        public DelegateCommand OpenEmailCommand { get; private set; }

        public DelegateCommand SelectOtherInstitutionCommand { get; private set; }

        public DelegateCommand ShowTermsOfUseCommand { get; private set; }

        public ProviderInfo InstitutionInfo => this.eapConfig.InstitutionInfo;

        public bool ShowProfileImage => this.ProfileImage != null;

        public BitmapImage? ProfileImage
        {
            get
            {
                if (this.InstitutionInfo.LogoData.Length > 0)
                {
                    var logoBytes = this.InstitutionInfo.LogoData;
                    var logoMimeType = this.InstitutionInfo.LogoMimeType;

                    if (logoMimeType != "image/svg+xml")
                    {
                        return ImageFunctions.LoadImage(logoBytes);
                    }
                }

                return null;
            }
        }

        public bool ShowProfileWebImage => !string.IsNullOrWhiteSpace(this.ProfileWebImage);

        public string? ProfileWebImage
        {
            get
            {
                if (this.InstitutionInfo.LogoData.Length > 0)
                {
                    var logoBytes = this.InstitutionInfo.LogoData;
                    var logoMimeType = this.InstitutionInfo.LogoMimeType;

                    if (logoMimeType == "image/svg+xml")
                    {
                        return ImageFunctions.GenerateSvgLogoHtml(logoBytes);
                    }

                }

                return null;
            }
        }

        public string Name => this.eapConfig.InstitutionInfo.DisplayName;

        public string Description => this.eapConfig.InstitutionInfo.Description;

        public bool HasContactInfo
        {
            get
            {
                return (this.HasWebAddress || this.HasEmailAddress || this.HasPhone);
            }
        }

        public bool HasWebAddress => !string.IsNullOrWhiteSpace(this.eapConfig.InstitutionInfo.WebAddress);
        public string WebAddress => this.eapConfig.InstitutionInfo.WebAddress;

        public bool WebAddressIsValid =>
            Uri.TryCreate(this.WebAddress, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        public bool HasEmailAddress => !string.IsNullOrWhiteSpace(this.eapConfig.InstitutionInfo.EmailAddress);

        public string EmailAddress => this.eapConfig.InstitutionInfo.EmailAddress;

        public bool EmailIsValid => !(this.EmailAddress.Contains(' ') || !this.EmailAddress.Contains('@'));

        public bool HasPhone => !string.IsNullOrWhiteSpace(this.eapConfig.InstitutionInfo.Phone);

        public string Phone => this.eapConfig.InstitutionInfo.Phone;

        public bool PhoneIsValid => !string.IsNullOrWhiteSpace(this.Phone);

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
            return string.IsNullOrWhiteSpace(this.Owner.State.SelectedProfile?.Redirect);
        }

        protected override Task NavigateNextAsync()
        {
            var configureTask = new ConfigureTask(this.eapConfig);
            var installers = configureTask.GetCertificateInstallers();

            if (installers.Any(installer => installer.IsInstalledByUs || !installer.IsInstalled))
            {
                this.Owner.SetActiveContent(new CertificateViewModel(this.Owner, this.eapConfig));
                return Task.CompletedTask;
            }

            this.Owner.Connect(this.eapConfig);
            return Task.CompletedTask;
        }

        private void ShowTermsOfUse()
        {
            this.Owner.SetActiveContent(new TermsOfUseViewModel(this.Owner, this.eapConfig));
        }
    }
}