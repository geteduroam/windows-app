using EduRoam.Connect.Eap;
using EduRoam.Connect.Identity;
using EduRoam.Connect.Language;

using System.Diagnostics;

namespace EduRoam.Connect.Tasks.Connectors
{
    public partial class CredentialsConnector : Connector
    {

        public CredentialsConnector(EapConfig eapConfig) : base(eapConfig)
        {
        }

        public override ConnectionType ConnectionType => ConnectionType.Credentials;

        public ConnectorCredentials? Credentials { get; set; }

        public TaskStatus ValidateCredentials()
        {
            var status = new TaskStatus(false);

            if (this.Credentials == null || string.IsNullOrWhiteSpace(this.Credentials.UserName) || this.Credentials.Password.Length == 0)
            {
                status.Errors.Add(Resource.ErrorInvalidCredentials);
                return status;
            }

            var (realm, hint) = this.eapConfig.GetClientInnerIdentityRestrictions();

            var brokenRules = IdentityProviderParser.GetRulesBrokenOnUsername(this.Credentials.UserName, realm, hint);
            if (brokenRules.Any())
            {
                status.Errors = brokenRules.ToList();
                return status;
            }

            if (this.eapConfig.RequiredAnonymousIdentRealm != null) // required realm can be empty string!
            {
                // Windows will set the realm itself for PEAP-EAP-MSCHAPv2
                // If the realm does not match, AND ALL OTHER TESTS ARE OK (no broken rules),
                // warn the user if the realms mismatch, but don't prevent connecting.
                var fullUsername = this.Credentials.UserName;
                var userRealm = fullUsername.Contains('@')
                    ? fullUsername.Substring(fullUsername.IndexOf('@'))
                    : "";

                if (this.eapConfig.RequiredAnonymousIdentRealm != userRealm)
                {
                    var strProfileRealm = string.IsNullOrEmpty(this.eapConfig.RequiredAnonymousIdentRealm)
                        ? "realmless"
                        : "\"" + this.eapConfig.RequiredAnonymousIdentRealm + "\"";

                    status.Errors.Add(string.Format(Resource.WarnRealmMismatch, userRealm, strProfileRealm));
                    return status;
                }
            }

            status.Success = true;
            return status;
        }

        public override async Task<TaskStatus> ConfigureAsync(bool forceConfiguration = false)
        {
            var status = this.ValidateCredentials();

            if (!status.Success)
            {
                return status;
            }

            status = await base.ConfigureAsync(forceConfiguration);

            if (status.Success)
            {
                var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(this.Credentials!.UserName!, this.Credentials!.Password);

                var exception = InstallEapConfig(eapConfigWithCredentials);
                if (exception != null)
                {
                    status.Success = false;
                    status.Errors.Add(exception.Message);
                }
            }

            return status;
        }

        public override async Task<TaskStatus> ConnectAsync()
        {
            var status = this.ValidateCredentials();

            if (!status.Success)
            {
                return status;
            }

            Debug.Assert(
                !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                "Cannot configure EAP config that still needs credentials"
            );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                status.Success = false;
                status.Errors.Add(Resource.ErrorWirelessUnavailable);
                return status;
            }

            var eapConfigWithCredentials = this.eapConfig.WithLoginCredentials(this.Credentials!.UserName!, this.Credentials.Password.ToString()!);

            status.Success = await Task.Run(ConnectToEduroam.TryToConnect);

            if (status.Success)
            {
                status.Messages.Add(Resource.Connected);
            }
            else
            {
                if (EduRoamNetwork.IsNetworkInRange(eapConfigWithCredentials))
                {
                    status.Errors.Add(Resource.ErrorConfiguredButUnableToConnect);
                }
                else
                {
                    // Hs2 is not enumerable
                    status.Errors.Add(Resource.ErrorConfiguredButProbablyOutOfCoverage);
                }
            }

            return status;
        }
    }
}
