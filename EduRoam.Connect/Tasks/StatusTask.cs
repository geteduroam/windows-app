using EduRoam.Connect.Store;

using System.Security.Principal;

namespace EduRoam.Connect.Tasks
{
    public class StatusTask
    {
        private readonly BaseConfigStore store = new RegistryStore();

        private IdentityProviderInfo? IdentityProvider => this.store.IdentityProvider;

        public Status GetStatus()
        {
            var status = new Status();

            if (this.IdentityProvider != null)
            {
                status.Identity = this.IdentityProvider.Value;
            }
            return status;
        }

        public static bool RunAsAdministrator
        {
            get
            {
                using var identity = WindowsIdentity.GetCurrent();

                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
