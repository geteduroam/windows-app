using Microsoft.Win32;

using Newtonsoft.Json;

using System.Collections.Immutable;
using System.Diagnostics;

namespace EduRoam.Connect.Store
{
    public class RegistryStore : BaseConfigStore
    {
        public RegistryStore() { }

        public static RegistryStore Instance => new RegistryStore();

        public override void AddConfiguredWLANProfile(WLANProfile profile)
        {
            var currentProfiles = GetValue<ImmutableHashSet<WLANProfile>>("ConfiguredWLANProfiles");

            if (currentProfiles == null)
            {
                currentProfiles = ImmutableHashSet.Create<WLANProfile>();
            }
            var newProfiles = currentProfiles.Add(profile);

            SetValue("ConfiguredWLANProfiles", newProfiles);
        }

        public override void AddInstalledCertificate(Certificate certificate)
        {
            var currentCertificates = GetValue<ImmutableHashSet<Certificate>>("InstalledCertificates");

            if (currentCertificates == null)
            {
                currentCertificates = ImmutableHashSet.Create<Certificate>();
            }
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

        private const string ns = "HKEY_CURRENT_USER\\Software\\geteduroam"; // Namespace in Registry

        private static T? GetValue<T>(string key, string defaultJson = "null")
        {
            try
            {
                var value = (string?)Registry.GetValue(ns, key, null) ?? defaultJson;

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (JsonReaderException)
            {
                return JsonConvert.DeserializeObject<T>(defaultJson);
            }
        }

        private static void SetValue<T>(string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            if (serialized != (string?)Registry.GetValue(ns, key, null)) // only write when we make a change
            {
                Debug.WriteLine("Write to {0}\\{1}: {2}", ns, key, serialized);
                Registry.SetValue(ns, key, serialized);
            }

            return;
        }
    }
}
