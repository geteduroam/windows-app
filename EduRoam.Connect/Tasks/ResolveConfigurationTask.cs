using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;

namespace EduRoam.Connect.Tasks
{
    public class ResolveConfigurationTask
    {
        private EapConfig? eapConfig;

        /// <summary>
        /// Connect by a institutes profile
        /// </summary>
        /// <param name="eapConfig"></param>
        /// <param name="forceConfiguration">
        ///     Force automatic configuration (for example install certificates) 
        ///     if the profile is not already configured (fully).
        /// </param>
        /// <exception cref="ArgumentException"
        /// <exception cref="ApiParsingException" />
        /// <exception cref="ApiUnreachableException" />
        /// <exception cref="UnknownInstituteException" />
        /// <exception cref="UnknownProfileException" />
        /// <exception cref="EduroamAppUserException"/>
        public bool ResolveConfiguration(EapConfig eapConfig, bool forceConfiguration = true)
        {
            if (eapConfig == null)
            {
                return false;
            }

            this.eapConfig = eapConfig;

            if (!this.CheckIfEapConfigIsSupported())
            {
                return false;
            }

            return this.ResolveCertificates(forceConfiguration);
        }

        private bool ResolveCertificates(bool forceConfiguration)
        {
            ConsoleExtension.WriteStatus("In order to continue the following certificates have to be installed.");
            var installers = ConnectToEduroam.EnumerateCAInstallers(this.eapConfig!).ToList();
            foreach (var installer in installers)
            {
                Console.WriteLine();
                ConsoleExtension.WriteStatus($"* {installer}, installed: {(installer.IsInstalled ? "✓" : "x")}");
                Console.WriteLine();
            }

            var certificatesNotInstalled = installers.Where(installer => !installer.IsInstalled);

            if (certificatesNotInstalled.Any())
            {
                if (!forceConfiguration)
                {
                    ConsoleExtension.WriteStatus("One or more certificates are not installed yet. Install the certificates? (y/N)");
                    return false;
                }
                else
                {
                    try
                    {
                        foreach (var installer in certificatesNotInstalled)
                        {
                            installer.AttemptInstallCertificate();
                        }
                    }
                    catch (UserAbortException)
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        private bool CheckIfEapConfigIsSupported()
        {
            if (!EduRoamNetwork.IsEapConfigSupported(this.eapConfig!))
            {
                ConsoleExtension.WriteError(
                    "The profile you have selected is not supported by this application.\nNo supported authentification method was found.");
                return false;
            }
            return true;
        }

    }
}
