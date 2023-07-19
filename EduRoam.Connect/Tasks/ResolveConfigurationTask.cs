using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Store;

namespace EduRoam.Connect.Tasks
{
    public class ResolveConfigurationTask
    {
        private EapConfig? eapConfig;

        private readonly BaseConfigStore store = new RegistryStore();

        public ResolveConfigurationTask(EapConfig eapConfig)
        {
            this.eapConfig = eapConfig;
        }

        /// <summary>
        /// Resolve configuration of the certificates. This is a prerequisite for <see cref="ResolveConfiguration(EapConfig, bool)"/>
        /// </summary>
        /// <param name="forceConfiguration"></param>
        /// <returns></returns>
        public bool ResolveCertificates(bool forceConfiguration)
        {
            var certificatesNotInstalled = this.GetNotInstalledCertificates();

            if (certificatesNotInstalled.Any())
            {
                if (!forceConfiguration)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        foreach (var installer in certificatesNotInstalled)
                        {
                            installer.AttemptInstallCertificate();

                            if (installer.IsInstalledByUs)
                            {
                                // Any CA that we have installed must also be removed by us when it is not needed anymore
                                this.store.AddInstalledCertificate(installer.Certificate);
                            }
                        }
                    }
                    catch (UserAbortException)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<CertificateInstaller> GetNotInstalledCertificates()
        {
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);
            return certificatesNotInstalled;
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="eapConfig"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        /// <exception cref="ArgumentException"
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        /// <exception cref="UnknownProfileException" />
        /// <exception cref="EduroamAppUserException"/>
        public bool ResolveConfiguration(bool forceConfiguration = true)
        {
            if (!this.CheckIfEapConfigIsSupported())
            {
                return false;
            }

            return !this.GetNotInstalledCertificates().Any();
        }

        private bool CheckIfEapConfigIsSupported()
        {
            if (!EduRoamNetwork.IsEapConfigSupported(this.eapConfig!))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
                return false;
            }
            return true;
        }

    }
}
