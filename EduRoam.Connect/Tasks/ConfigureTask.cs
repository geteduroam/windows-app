using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Store;
using EduRoam.Connect.Tasks.Connectors;
using EduRoam.Localization;

using System;
using System.Collections.Generic;
using System.Linq;

namespace EduRoam.Connect.Tasks
{
    public class ConfigureTask
    {
        private readonly EapConfig? eapConfig;

        private readonly BaseConfigStore store = new RegistryStore();

        public ConfigureTask() : this(null)
        { }

        public ConfigureTask(EapConfig? eapConfig)
        {
            this.eapConfig = eapConfig;
        }

        public IList<CertificateInstaller> GetCertificateInstallers()
        {
            if (this.eapConfig == null)
            {
                throw new ArgumentException(Resources.ErrorEapConfigIsEmpty);
            }

            return ConnectToEduroam.EnumerateCAInstallers(this.eapConfig).ToList();
        }

        public TaskStatus ConfigureCertificate(CertificateInstaller installer)
        {
            installer.AttemptInstallCertificate();

            if (installer.IsInstalledByUs)
            {
                // Any CA that we have installed must also be removed by us when it is not needed anymore
                this.store.AddInstalledCertificate(installer.Certificate);
            }

            return TaskStatus.AsSuccess();
        }

        public Connector? GetConnector()
        {
            return Connector.GetInstance(this.eapConfig);
        }

        private IEnumerable<CertificateInstaller> GetNotInstalledCertificates()
            => ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).Where(installer => !installer.IsInstalled);
    }
}
