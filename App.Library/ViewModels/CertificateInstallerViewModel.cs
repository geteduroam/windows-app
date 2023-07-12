using App.Library.Command;

using EduRoam.Connect;

using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class CertificateInstallerViewModel : NotifyPropertyChanged
    {
        public CertificateInstallerViewModel(CertificateInstaller certificateInstaller)
        {
            this.CertificateInstaller = certificateInstaller;
            this.InstallCommand = new DelegateCommand(this.Install, this.CanInstall);
        }

        public DelegateCommand InstallCommand { get; protected set; }

        public CertificateInstaller CertificateInstaller { get; private set; }

        public string Text => this.CertificateInstaller.ToString();

        public bool IsInstalled => this.CertificateInstaller.IsInstalled;

        private bool CanInstall()
        {
            return true;
        }

        private void Install()
        {
            this.CertificateInstaller.AttemptInstallCertificate();
            if (this.CertificateInstaller.IsInstalledByUs)
            {
                // Any CA that we have installed must also be removed by us when it is not needed anymore
                // so install the geteduroam app when we have installed a CA
                _ = Task.Run(MainViewModel.SelfInstaller.EnsureIsInstalled);
            }

            this.CallPropertyChanged();
        }
    }
}