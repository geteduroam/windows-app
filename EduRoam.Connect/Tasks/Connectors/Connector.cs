using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;
using EduRoam.Connect.Language;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EduRoam.Connect.Tasks.Connectors
{
    public abstract class Connector
    {
        protected EapConfig eapConfig;

        protected Connector(EapConfig eapConfig)
        {
            this.eapConfig = eapConfig;
        }

        public static Connector? GetInstance(EapConfig? eapConfig)
        {
            if (CheckIfEapConfigIsSupported(eapConfig))
            {
                return GetConnectorAsync(eapConfig);
            }

            return null;
        }

        public abstract ConnectionType ConnectionType { get; }

        public virtual Task<(bool, IList<string>)> ConfigureAsync(bool forceConfiguration = false)
        {
            var certificatesNotInstalled = this.GetNotInstalledCertificates();

            var succes = !certificatesNotInstalled.Any();
            var message = succes ? Resource.ConfiguredEap : Resource.ErrorRequiredCertificatesNotInstalled;

            return Task.FromResult<(bool, IList<string>)>((succes, message.AsListItem()));
        }

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <returns>True if a connection could be established, false otherwise</returns>
        /// <exception cref="EduroamAppUserException" />
        public virtual async Task<(bool connected, IList<string> messages)> ConnectAsync()
        {
            Debug.Assert(
                    !this.eapConfig.NeedsClientCertificatePassphrase && !this.eapConfig.NeedsLoginCredentials,
                    "Cannot configure EAP config that still needs credentials"
                );

            if (!EduRoamNetwork.IsWlanServiceApiAvailable())
            {
                // TODO: update this when wired x802 is a thing
                return (false, Resource.ErrorWirelessUnavailable.AsListItem());
            }

            var connected = await Task.Run(ConnectToEduroam.TryToConnect);
            var message = string.Empty;

            if (connected)
            {
                message = Resource.Connected;
            }
            else
            {
                if (this.eapConfig == null)
                {
                    message = Resource.ErrorConfiguredButNotConnected;

                }
                else if (EduRoamNetwork.IsNetworkInRange(this.eapConfig))
                {
                    message = Resource.ErrorConfiguredButUnableToConnect;
                }
                else
                {
                    // Hs2 is not enumerable
                    message = Resource.ErrorConfiguredButProbablyOutOfCoverage;
                }
            }

            return (connected, message.AsListItem());
        }

        protected static bool CheckIfEapConfigIsSupported([NotNullWhen(true)] EapConfig? eapConfig)
        {
            if (eapConfig == null)
            {
                return false;
            }

            if (!EduRoamNetwork.IsEapConfigSupported(eapConfig))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
                return false;
            }
            return true;
        }

        private IEnumerable<CertificateInstaller> GetNotInstalledCertificates()
        {
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            return installers.Where(installer => !installer.IsInstalled);
        }

        protected Exception? InstallEapConfig(EapConfig eapConfig)
        {
            if (!CheckIfEapConfigIsSupported(eapConfig)) // should have been caught earlier, but check here too for sanity
            {
                throw new Exception(Resource.ErrorInvalidEapConfig);
            }

            ConnectToEduroam.RemoveAllWLANProfiles();

            Exception? lastException = null;
            // Install EAP config as a profile
            foreach (var authMethod in eapConfig.SupportedAuthenticationMethods)
            {
                lastException = null;
                var authMethodInstaller = new EapAuthMethodInstaller(authMethod);

                // install intermediate CAs and client certificates
                // if user refuses to install a root CA (should never be prompted to at this stage), abort
                try
                {
                    authMethodInstaller.InstallCertificates();

                    // Everything is now in order, install the profile!
                    authMethodInstaller.InstallWLANProfile();

                    break;
                }
                catch (UserAbortException ex)
                {
                    lastException = new Exception(Resource.ErrorMissingRequiredCACertificate, ex);
                    // failed, try the next method
                }
                catch (Exception e)
                {
                    lastException = e;
                    // failed, try the next method
                }
            }

            return lastException;
        }

        private static Connector? GetConnectorAsync(EapConfig eapConfig)
        {
            if (eapConfig == null)
            {
                return null;
            }

            if (eapConfig.NeedsLoginCredentials)
            {
                return new CredentialsConnector(eapConfig);
            }
            else if (eapConfig.NeedsClientCertificate)
            {
                return new CertAndCertPassConnector(eapConfig);

            }
            // case where eapconfig needs only cert password
            else if (eapConfig.NeedsClientCertificatePassphrase)
            {
                return new CertPassConnector(eapConfig);
            }
            else
            {
                return new DefaultConnector(eapConfig);
            }
        }
    }
}
