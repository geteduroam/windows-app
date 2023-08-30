using System.Collections.Immutable;

namespace EduRoam.Connect.Store
{
    public abstract class BaseConfigStore
    {
        public BaseConfigStore()
        {
            this.ConfiguredWLANProfiles = ImmutableHashSet.Create<WLANProfile>();
            this.InstalledCertificates = ImmutableHashSet.Create<Certificate>();

        }

        /// <summary>
        /// The username to remember from when the user last logged in
        /// </summary>
        public virtual string? Username { get; }

        public virtual IdentityProviderInfo? IdentityProvider { get; }

        /// <summary>
        /// A set of the configured WLANProfiles
        /// </summary>
        public virtual ImmutableHashSet<WLANProfile> ConfiguredWLANProfiles { get; }

        /// <summary>
        /// A set of the installed CAs and client certificates.
        /// Managed by EduroamConfigure.CertificateStore
        /// </summary>
        public virtual ImmutableHashSet<Certificate> InstalledCertificates { get; }

        /// <summary>
        /// The endpoints to access the lets-wifi
        /// using the refresh token
        /// </summary>
        public virtual WifiEndpoint? WifiEndpoint { get; }

        /// <summary>
        /// The single-use refresh token to talk with the lets-wifi API
        /// </summary>
        public virtual string? WifiRefreshToken { get; }

        public abstract void UpdateIdentity(string userName, IdentityProviderInfo provider);

        public abstract void UpdateIdentity(IdentityProviderInfo? provider);

        public abstract void ClearIdentity();

        public abstract void AddConfiguredWLANProfile(WLANProfile profile);

        public abstract void RemoveConfiguredWLANProfile(WLANProfile profile);

        public abstract void AddInstalledCertificate(Certificate certificate);

        public abstract void RemoveInstalledCertificate(Certificate certificate);

        public abstract void UpdateWifiEndpoint(WifiEndpoint? endpoint);

        public abstract void ClearWifiEndpoint();

        public abstract void UpdateWifiRefreshToken(string? refreshToken);

        public abstract void ClearWifiRefreshToken();

        /// <summary>
        /// Check if there is installed any valid (not neccesarily tested) connection
        /// </summary>
        public bool AnyValidWLANProfile
        {
            get => this.ConfiguredWLANProfiles.Any(p => p.HasUserData);
        }

        /// <summary>
        /// True if the currently installed wlanprofile is the one we have stored refresh credentials for
        /// </summary>
        public bool IsRefreshable
        {
            get => this.WifiEndpoint?.ProfileId == this.IdentityProvider?.ProfileId
                && !string.IsNullOrEmpty(this.IdentityProvider?.ProfileId)
                && !string.IsNullOrEmpty(this.WifiRefreshToken);
        }

        /// <summary>
        /// True if we have a eapconfig xml file stored.
        /// Usually only applies for PEAP and TTLS gotten from either a file or from discovery.
        /// This allows the user to reinstall the profile without having to redownload the config file.
        /// </summary>
        public bool IsReinstallable
        {
            get => !string.IsNullOrEmpty(this.IdentityProvider?.ProfileId)
                && !string.IsNullOrEmpty(this.IdentityProvider?.EapConfigXml);
        }
    }
}
