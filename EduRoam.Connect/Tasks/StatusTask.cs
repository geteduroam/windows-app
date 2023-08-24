using EduRoam.Connect.Store;

namespace EduRoam.Connect.Tasks
{
    public class StatusTask
    {
        private readonly BaseConfigStore store = new RegistryStore();

        private IdentityProviderInfo? IdentityProvider
        {
            get
            {
                return this.store.IdentityProvider;
            }
        }

        public Status GetStatus()
        {
            var status = new Status();

            if (this.IdentityProvider != null)
            {
                status.Identity = this.IdentityProvider.Value;
            }
            return status;
        }
    }
}
