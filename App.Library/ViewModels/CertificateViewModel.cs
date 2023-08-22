using EduRoam.Connect.Eap;
using EduRoam.Connect.Tasks;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
                installers.Select(x => new CertificateInstallerViewModel(x)));
            this.eapConfig = eapConfig;
        }

        public ObservableCollection<CertificateInstallerViewModel> Installers { get; }

        public bool AllCertificatesAreInstalled => this.Installers.All(x => x.IsInstalled);

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