using Microsoft.Win32;

using Newtonsoft.Json;

using System.Collections.Immutable;
using System.Diagnostics;

namespace EduRoam.Connect.Store
{
    public class RegistryStore : BaseConfigStore
    {
        private const string UserNameEntry = "Username";
        private const string IdentityProviderEntry = "IdentityProvider";
        private const string ConfiguredWLANProfilesEntry = "ConfiguredWLANProfiles";
        private const string InstalledCertificatesEntry = "InstalledCertificates";
        private const string LetsWifiEndpointsEntry = "LetsWifiEndpoints";
        private const string LetsWifiRefreshTokenEntry = "LetsWifiRefreshToken";

        public RegistryStore() : base()
        { }

        public override string? Username => GetValue<string>(UserNameEntry);

        public override IdentityProviderInfo? IdentityProvider => GetValue<IdentityProviderInfo?>(IdentityProviderEntry);

        public override ImmutableHashSet<WLANProfile> ConfiguredWLANProfiles => GetValue<ImmutableHashSet<WLANProfile>>(ConfiguredWLANProfilesEntry) ?? ImmutableHashSet<WLANProfile>.Empty;

        public override ImmutableHashSet<Certificate> InstalledCertificates => GetValue<ImmutableHashSet<Certificate>>(InstalledCertificatesEntry) ?? ImmutableHashSet<Certificate>.Empty;

        public override WifiEndpoint? WifiEndpoint => GetValue<WifiEndpoint?>(LetsWifiEndpointsEntry);

        public override string? WifiRefreshToken => GetValue<string?>(LetsWifiRefreshTokenEntry);

        public static RegistryStore Instance => new();

        public override void AddConfiguredWLANProfile(WLANProfile profile)
        {
            var currentProfiles = GetValue<ImmutableHashSet<WLANProfile>>(ConfiguredWLANProfilesEntry) ?? ImmutableHashSet<WLANProfile>.Empty;

            var newProfiles = currentProfiles.Add(profile);

            SetValue(ConfiguredWLANProfilesEntry, newProfiles);
        }

        public override void RemoveConfiguredWLANProfile(WLANProfile profile)
        {
            var currentProfiles = GetValue<ImmutableHashSet<WLANProfile>>(ConfiguredWLANProfilesEntry);

            if (currentProfiles == null)
            {
                return;
            }
            var newProfiles = currentProfiles.Remove(profile);

            SetValue(ConfiguredWLANProfilesEntry, newProfiles);
        }

        public override void AddInstalledCertificate(Certificate certificate)
        {
            var currentCertificates = GetValue<ImmutableHashSet<Certificate>>(InstalledCertificatesEntry) ?? ImmutableHashSet<Certificate>.Empty;

            var newCertificates = currentCertificates.Add(certificate);

            SetValue(InstalledCertificatesEntry, newCertificates);
        }

        public override void RemoveInstalledCertificate(Certificate certificate)
        {
            var currentCertificates = GetValue<ImmutableHashSet<Certificate>>(InstalledCertificatesEntry);

            if (currentCertificates == null)
            {
                return;
            }
            var newCertificates = currentCertificates.Remove(certificate);

            SetValue(InstalledCertificatesEntry, newCertificates);
        }

        public override void UpdateIdentity(string userName, IdentityProviderInfo provider)
        {
            SetValue(UserNameEntry, userName);
            SetValue(IdentityProviderEntry, provider);
        }

        public override void UpdateIdentity(IdentityProviderInfo? provider)
        {
            SetValue(IdentityProviderEntry, provider);
        }

        public override void ClearIdentity()
        {
            this.UpdateIdentity(null);
        }

        public override void UpdateWifiEndpoint(WifiEndpoint? endpoint)
        {
            SetValue(LetsWifiEndpointsEntry, endpoint);
        }

        public override void ClearWifiEndpoint()
        {
            this.UpdateWifiEndpoint(null);
        }

        public override void UpdateWifiRefreshToken(string? refreshToken)
        {
            // todo: perhaps encrypt this in some fashion?
            // https://stackoverflow.com/questions/32548714/how-to-store-and-retrieve-credentials-on-windows-using-c-sharp
            SetValue(LetsWifiRefreshTokenEntry, refreshToken);
        }

        public override void ClearWifiRefreshToken()
        {
            this.UpdateWifiRefreshToken(null);
        }

        private const string AppRegistryNamespace = "HKEY_CURRENT_USER\\Software\\geteduroam";

        private static T? GetValue<T>(string key)
        {
            try
            {
                var value = (string?)Registry.GetValue(AppRegistryNamespace, key, null);

                if (value == null || value == "null")
                {
                    return default;
                }
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (JsonReaderException)
            {
                return default;
            }
        }

        private static void SetValue<T>(string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            if (serialized != (string?)Registry.GetValue(AppRegistryNamespace, key, null)) // only write when we make a change
            {
                Debug.WriteLine("Write to {0}\\{1}: {2}", AppRegistryNamespace, key, serialized);
                Registry.SetValue(AppRegistryNamespace, key, serialized);
            }
        }
    }
}
