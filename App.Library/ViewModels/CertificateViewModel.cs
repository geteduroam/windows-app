using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace App.Library.ViewModels
{
    public class CertificateViewModel : BaseViewModel
    {
        private readonly EapConfig eapConfig;

        public CertificateViewModel(MainViewModel owner, EapConfig eapConfig)
            : base(owner)
        {
            //todo maybe subscribe to NotifyChanged or custom event to trigger AllInstalled
            var configureTask = new ConfigureTask(eapConfig);
            var installers = configureTask.GetCertificateInstallers();

            this.Installers = new ObservableCollection<CertificateInstallerViewModel>(
                installers.Select(installer => new CertificateInstallerViewModel(installer)));
            this.eapConfig = eapConfig;

            this.Owner.PropertyChanged += SkipInstallCertificatesIfInstalled;

        }

        public void SkipInstallCertificatesIfInstalled(object sender, PropertyChangedEventArgs e)
        {
            this.Owner.PropertyChanged -= SkipInstallCertificatesIfInstalled;
            if (this.CanNavigateNextAsync())
            {
                this.NavigateNextAsync();
            }
        }

        public override string PageTitle => string.Empty;

        public ObservableCollection<CertificateInstallerViewModel> Installers { get; }

        public bool AllCertificatesAreInstalled => this.Installers.All(installer => installer.IsInstalled);


        protected override bool CanNavigateNextAsync()
        {
            return this.AllCertificatesAreInstalled;
        }

        protected override Task NavigateNextAsync()
        {
            this.Owner.Connect(this.eapConfig);
            return Task.CompletedTask;
        }
    }
}