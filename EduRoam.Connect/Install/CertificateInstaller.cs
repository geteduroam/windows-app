using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Install;
using EduRoam.Connect.Store;

using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect
{

    /// <summary>
    /// A helper class which helps you ensure a single certificates is installed.
    /// </summary>
    public class CertificateInstaller
    {
        private readonly X509Certificate2 cert;
        private readonly StoreName storeName;
        private readonly StoreLocation storeLocation;

        public CertificateInstaller(
            X509Certificate2 cert,
            StoreName storeName,
            StoreLocation storeLocation)
        {
            this.cert = cert ?? throw new ArgumentNullException(paramName: nameof(cert));
            this.storeLocation = storeLocation;
            this.storeName = storeName;
        }

        public override string ToString() => this.cert.FriendlyName;

        public bool IsCa { get => this.storeName == CertificateStore.RootCaStoreName; }

        public bool IsInstalled
        {
            get => CertificateStore.IsCertificateInstalled(this.cert, this.storeName, this.storeLocation);
        }
        public bool IsInstalledByUs
        {
            get => CertificateStore.IsCertificateInstalledByUs(this.cert, this.storeName, this.storeLocation);
        }

        public Certificate Certificate
        {
            get => Certificate.FromCertificate(this.cert, this.storeName, this.storeLocation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="UserAbortException" />
        public void AttemptInstallCertificate()
        {
            CertificateStore.InstallCertificate(this.cert, this.storeName, this.storeLocation);
        }
    }

}
