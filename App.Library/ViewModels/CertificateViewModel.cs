using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using EduRoam.Connect;

namespace App.Library.ViewModels
{
    public class CertificateViewModel : BaseViewModel
    {
        public CertificateViewModel(MainViewModel mainViewModel, EapConfig eapConfig)
            : base(mainViewModel)
        {
            //todo maybe subscribe to NotifyChanged or custom event to trigger AllInstalled
            this.Installers = new ObservableCollection<CertificateInstallerViewModel>(
                ConnectToEduroam.EnumerateCAInstallers(eapConfig)
                                .Select(x => new CertificateInstallerViewModel(x)));
        }

        public ObservableCollection<CertificateInstallerViewModel> Installers { get; }

        public bool AllCertificatesAreInstalled => Installers.All(x => x.IsInstalled);

        protected override bool CanGoNext()
        {
            return this.AllCertificatesAreInstalled;
        }

        protected override Task GoNextAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}