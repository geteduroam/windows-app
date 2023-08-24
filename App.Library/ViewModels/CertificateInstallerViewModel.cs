using App.Library.Command;

using EduRoam.Connect;
using EduRoam.Connect.Tasks;

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
            var configurator = new ConfigureTask();
            configurator.ConfigureCertificate(this.CertificateInstaller);

            this.CallPropertyChanged(nameof(this.IsInstalled));
        }
    }
}