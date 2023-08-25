﻿using EduRoam.Connect.Install;
using EduRoam.Connect.Store;

namespace EduRoam.Connect.Tasks
{
    public class RemoveWiFiConfigurationTask
    {
        /// <summary>
		/// Uninstalls the installed WLAN profile
		/// </summary>
		/// <param name="omitRootCa">Keep the installed root certificate</param>
		/// <remarks>
		/// On one hand, keeping the root is a security risk,
		/// on the other, it's a hassle for the user to get too many prompts
		/// while reinstalling a profile.
		/// </remarks>
		public static void Remove(bool omitRootCa = false)
        {
            LetsWifi.WipeTokens();
            ConnectToEduroam.RemoveAllWLANProfiles();
            RegistryStore.Instance.ClearIdentity();
            RemoveCertificates(omitRootCa: omitRootCa);
        }

        public static void RemoveCertificates(bool omitRootCa = false)
        {
            CertificateStore.UninstallAllInstalledCertificates(omitRootCa: omitRootCa);
        }
    }
}
