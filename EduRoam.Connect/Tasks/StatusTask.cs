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
                status.ProfileName = this.IdentityProvider.Value.DisplayName;
                status.ExpirationDate = this.IdentityProvider.Value.NotAfter;
            }
            return status;
        }
    }
}
