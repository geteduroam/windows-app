using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Install;

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
        public EapConfig.AuthenticationMethod AuthMethod { get; }


        /// <summary>
        /// Constructs a EapAuthMethodInstaller
        /// </summary>
        /// <param name="authMethod">The authentification method to attempt to install</param>
        public EapAuthMethodInstaller(EapConfig.AuthenticationMethod authMethod)
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
            if (AuthMethod.NeedsClientCertificate)
            {
                throw new EduroamAppUserException("no client certificate was provided");
            }

            // get all CAs from Authentication method
            foreach (var cert in AuthMethod.CertificateAuthoritiesAsX509Certificate2())
            {
                // if this doesn't work, try https://stackoverflow.com/a/34174890
                bool isRootCA = cert.Subject == cert.Issuer;
                CertificateStore.InstallCertificate(cert,
                    isRootCA ? CertificateStore.RootCaStoreName : CertificateStore.InterCaStoreName,
                    isRootCA ? CertificateStore.RootCaStoreLocation : CertificateStore.InterCaStoreLocation);
            }

            // Install client certificate if any
            if (!string.IsNullOrEmpty(AuthMethod.ClientCertificate))
            {
                using var clientCert = AuthMethod.ClientCertificateAsX509Certificate2();
                CertificateStore.InstallCertificate(clientCert, CertificateStore.UserCertStoreName, CertificateStore.UserCertStoreLocation);
            }

            hasInstalledCertificates = true;
        }

        /// <summary>
        /// Will install the authMethod as a profile
        /// Having run InstallCertificates successfully before calling this is a prerequisite
        /// If this returns FALSE: It means there is a missing TLS client certificate left to be installed
        /// </summary>
        /// <returns>True if the profile was installed on any interface</returns>
        public void InstallWLANProfile()
        {
            if (!hasInstalledCertificates)
                throw new EduroamAppUserException("missing certificates",
                    "You must first install certificates with InstallCertificates");

            // Install wlan profile
            foreach (var network in EduRoamNetwork.GetAll(AuthMethod.EapConfig))
            {
                Debug.WriteLine("Install profile {0}", network.ProfileName);
                network.InstallProfiles(AuthMethod, forAllUsers: true);
            }
        }

        public (DateTime From, DateTime? To) GetTimeWhenValid()
        {
            using var cert = AuthMethod.ClientCertificateAsX509Certificate2();
            return cert == null
                ? (DateTime.Now.AddSeconds(-30), (DateTime?)null)
                : (cert.NotBefore, cert.NotAfter);
        }

    }


}
