using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Install;
using EduRoam.Localization;

using System;
using System.Diagnostics;

namespace EduRoam.Connect
{

    /// <summary>
    /// A class which helps you install one of the authMethods
    /// in a EapConfig, designed to be interactive wiht the user.
    /// </summary>
    public class EapAuthMethodInstaller
    {
        // To track proper order of operations
        private bool hasInstalledCertificates = false;

        // reference to the EAP config
        public Eap.AuthenticationMethod AuthMethod { get; }

        /// <summary>
        /// Constructs a EapAuthMethodInstaller
        /// </summary>
        /// <param name="authMethod">The authentification method to attempt to install</param>
        public EapAuthMethodInstaller(Eap.AuthenticationMethod authMethod)
        {
            this.AuthMethod = authMethod ?? throw new ArgumentNullException(paramName: nameof(authMethod));
        }

        /// <summary>
        /// Will install root CAs, intermediate CAs and user certificates provided by the authMethod.
        /// Installing a root CA in windows will produce a dialog box which the user must accept.
        /// This will quit partway through if the user refuses to install any CA, but it is safe to run again.
        /// Use EnumerateCAInstallers to have the user install the CAs in a controlled manner before installing the EAP config
        /// </summary>
        /// <returns>Returns true if all certificates has been successfully installed</returns>
        public void InstallCertificates()
        {
            if (this.AuthMethod.NeedsClientCertificate)
            {
                throw new EduroamAppUserException(Resources.ErrorNoClientCertificateProvided);
            }

            // get all CAs from Authentication method
            foreach (var cert in this.AuthMethod.CertificateAuthoritiesAsX509Certificate2())
            {
                // if this doesn't work, try https://stackoverflow.com/a/34174890
                var isRootCA = cert.Subject == cert.Issuer;
                CertificateStore.InstallCertificate(cert,
                    isRootCA ? CertificateStore.RootCaStoreName : CertificateStore.InterCaStoreName,
                    CertificateStore.CertStoreLocation);
            }

            // Install client certificate if any
            if (!string.IsNullOrEmpty(this.AuthMethod.ClientCertificate))
            {
                using var clientCert = this.AuthMethod.ClientCertificateAsX509Certificate2();

                if (clientCert != null)
                {
                    CertificateStore.InstallCertificate(clientCert, CertificateStore.UserCertStoreName, CertificateStore.UserCertStoreLocation);
                    CertificateStore.InstallCertificate(clientCert, CertificateStore.UserCertStoreName, CertificateStore.CertStoreLocation);
                }
            }

            this.hasInstalledCertificates = true;
        }

        /// <summary>
        /// Will install the authMethod as a profile
        /// Having run InstallCertificates successfully before calling this is a prerequisite
        /// If this returns FALSE: It means there is a missing TLS client certificate left to be installed
        /// </summary>
        /// <returns>True if the profile was installed on any interface</returns>
        public void InstallWLANProfile()
        {
            if (!this.hasInstalledCertificates)
            {
                throw new EduroamAppUserException(Resources.WarningMissingCertificates,
                    Resources.WarningInstallCertificates);
            }

            // Install wlan profile
            foreach (var network in EduRoamNetwork.GetAll(this.AuthMethod.EapConfig))
            {
                Debug.WriteLine("Install profile {0}", network.ProfileName);
                network.InstallProfiles(this.AuthMethod, forAllUsers: true);
            }
        }

        public (DateTime From, DateTime? To) GetTimeWhenValid()
        {
            using var cert = this.AuthMethod.ClientCertificateAsX509Certificate2();
            return cert == null
                ? (DateTime.Now.AddSeconds(-30), null)
                : (cert.NotBefore, cert.NotAfter);
        }
    }
}
