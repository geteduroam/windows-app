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
        public TaskStatus ConfigureCertificates(bool forceConfiguration)
        {
            var certificatesNotInstalled = this.GetNotInstalledCertificates();

            if (certificatesNotInstalled.Any())
            {
                if (!forceConfiguration)
                {
                    return TaskStatus.AsFailure();
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
                        return TaskStatus.AsFailure();
                    }
                }
            }

            return TaskStatus.AsSuccess();
        }

        public Connector? GetConnector()
        {
            return Connector.GetInstance(this.eapConfig);
        }

        private IEnumerable<CertificateInstaller> GetNotInstalledCertificates()
        {
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);
            return certificatesNotInstalled;
        }
    }
}
