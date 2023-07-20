using Microsoft.Win32;

using Newtonsoft.Json;

using System.Collections.Immutable;
using System.Diagnostics;

namespace EduRoam.Connect.Store
{
    public class RegistryStore : BaseConfigStore
    {
        public RegistryStore() : base()
        {
            this.Username = GetValue<string>("Username");
            this.IdentityProvider = GetValue<IdentityProviderInfo?>("IdentityProvider");
            this.ConfiguredWLANProfiles = GetValue<ImmutableHashSet<WLANProfile>>("ConfiguredWLANProfiles") ?? ImmutableHashSet<WLANProfile>.Empty;
            this.InstalledCertificates = GetValue<ImmutableHashSet<Certificate>>("InstalledCertificates") ?? ImmutableHashSet<Certificate>.Empty;
            this.WifiEndpoint = GetValue<WifiEndpoint?>("LetsWifiEndpoints");
            this.WifiRefreshToken = GetValue<string?>("LetsWifiRefreshToken");

        }

        public static RegistryStore Instance => new();

        public override void AddConfiguredWLANProfile(WLANProfile profile)
        {
            var currentProfiles = GetValue<ImmutableHashSet<WLANProfile>>("ConfiguredWLANProfiles") ?? ImmutableHashSet<WLANProfile>.Empty;

            var newProfiles = currentProfiles.Add(profile);

            SetValue("ConfiguredWLANProfiles", newProfiles);
        }

        public override void AddInstalledCertificate(Certificate certificate)
        {
            var currentCertificates = GetValue<ImmutableHashSet<Certificate>>("InstalledCertificates") ?? ImmutableHashSet<Certificate>.Empty;

            var newCertificates = currentCertificates.Add(certificate);

            SetValue("InstalledCertificates", newCertificates);
        }

        public override void RemoveConfiguredWLANProfile(WLANProfile profile)
        {
            var currentProfiles = GetValue<ImmutableHashSet<WLANProfile>>("ConfiguredWLANProfiles");

            if (currentProfiles == null)
            {
                return;
            }
            var newProfiles = currentProfiles.Remove(profile);

            SetValue("ConfiguredWLANProfiles", newProfiles);
        }

        public override void RemoveInstalledCertificate(Certificate certificate)
        {
            var currentCertificates = GetValue<ImmutableHashSet<Certificate>>("InstalledCertificates");

            if (currentCertificates == null)
            {
                return;
            }
            var newCertificates = currentCertificates.Remove(certificate);

            SetValue("InstalledCertificates", newCertificates);
        }

        public override void UpdateIdentity(string userName, IdentityProviderInfo provider)
        {
            SetValue("Username", userName);
            SetValue("IdentityProvider", provider);
        }

        public override void UpdateIdentity(IdentityProviderInfo? provider)
        {
            SetValue("IdentityProvider", provider);
        }

        public override void ClearIdentity()
        {
            this.UpdateIdentity(null);
        }

        public override void UpdateWifiEndpoint(WifiEndpoint? endpoint)
        {
            SetValue("LetsWifiEndpoints", endpoint);
        }

        public override void ClearWifiEndpoint()
        {
            this.UpdateWifiEndpoint(null);
        }

        public override void UpdateWifiRefreshToken(string? refreshToken)
        {
            // todo: perhaps encrypt this in some fashion?
            // https://stackoverflow.com/questions/32548714/how-to-store-and-retrieve-credentials-on-windows-using-c-sharp
            SetValue("LetsWifiRefreshToken", refreshToken);
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

            return;
        }
    }
}
