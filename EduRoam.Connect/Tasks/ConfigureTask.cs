using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Store;
using EduRoam.Connect.Tasks.Connectors;

namespace EduRoam.Connect.Tasks
{
    public class ConfigureTask
    {
        private EapConfig? eapConfig;

        private readonly BaseConfigStore store = new RegistryStore();

        public ConfigureTask(EapConfig eapConfig)
        {
            this.eapConfig = eapConfig;
        }

        /// <summary>
        /// Resolve configuration of the certificates. This is a prerequisite for <see cref="ResolveConfiguration(EapConfig, bool)"/>
        /// </summary>
        /// <param name="forceConfiguration"></param>
        /// <returns></returns>
        public bool ConfigureCertificates(bool forceConfiguration)
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

        public Connector? GetConnector(EapConfig eapConfig)
        {
            return Connector.GetInstance(eapConfig);
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="eapConfig"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        public async Task<bool> ConfigureAsyncOld(bool forceConfiguration = false)
        {
            var connector = Connector.GetInstance(this.eapConfig);
            if (connector != null)
            {
                var (succeeded, _) = await connector.ConfigureAsync(forceConfiguration);

                return succeeded;
            }

            return false;
        }

        private IEnumerable<CertificateInstaller> GetNotInstalledCertificates()
        {
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);
            return certificatesNotInstalled;
        }
    }
}
