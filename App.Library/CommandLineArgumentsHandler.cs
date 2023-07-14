using EduRoam.Connect.Install;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace App.Library
{
    public static class CommandLineArgumentsHandler
    {
        /// <summary>
        /// Handles command line args not related to wpf behaviour
        /// </summary>
        /// <returns>true if startup is to be aborted</returns>
        public static bool PreGuiCommandLineArgs(string[] args)
        {
            // shorthand
            bool contains(string check) =>
                args.Any(param => string.Equals(param, check, StringComparison.InvariantCultureIgnoreCase));

            if (contains("/?")
                || contains("/help"))
            {
                ShowHelpText();
                return true;
            }

            if (contains("/install")) // todo: MessageBox.Show(yes/no)
            {
                // Installer.EnsureIsInstalled();
                return true;
            }

            if (contains("/uninstall"))
            {
                PromptAndUninstallSelf(
                    success =>
                    {
                        Environment.Exit(0);
                    });
                return true;
            }

            if (contains("/refresh")
                || contains("/force-refresh")
                || contains("/refresh-force"))
            {
                RefreshInstalledProfile(force: contains("/refresh-force"));

                return true;
            }

            return contains("/close")
                   // Just quit when being started with /background
                   || contains("/background");
        }

        private static void ShowHelpText() =>
            MessageBox.Show(
                string.Join(
                    "\n",
                    new List<string>
                    {
                        "Supported CLI commands:",
                        "",
                        "    /? : ",
                        "            Show this help text",
                        "    /help : ",
                        "            Show this help text",
                        "    /install : ",
                        "            Install this binary to %USER%/AppData/Local",
                        "    /uninstall : ",
                        "            Uninstall this binary from %USER%/AppData/Local along",
                        "            with any configured data",
                        "    /background : ",
                        "            Start this application hidden to the tray",
                        "            (works only if run from install directory)",
                        "    /close : ",
                        "            Close the current running instance",
                        "    /refresh : ",
                        "            Refresh the user certificate using the refresh token.",
                        "            Is called automatically by a scheduled task in windows.",
                        "    /refresh-force : ",
                        "            Refresh the user certificate using the refresh token.",
                        "            Will refresh the profile even if the validity period",
                        "            of the current client certificate has more than a 3rd left",
                    }),
                caption: "geteduroam");

        /// <summary>
        /// Refresh the profile if needed and report to the user if running persistent
        /// </summary>
        /// <param name="f">Function to show user a message in the tray icon</param>
        /// <param name="force">Wether to force a reinstall even if the current certificate still is valid for quote some time</param>
        /// <exception cref="ApiException">The API did something unexpected</exception>
        private static async void RefreshInstalledProfile(bool force)
        {
            //            _ = await LetsWifi.RefreshAndInstallEapConfig(force) switch
            //            {
            //                LetsWifi.RefreshResponse.UpdatedEapXml => true, // fine

            //                LetsWifi.RefreshResponse.Success => true, // nice!
            //                LetsWifi.RefreshResponse.StillValid => true, // no work needed
            //                LetsWifi.RefreshResponse.NotRefreshable => false, // ignore, since we currently always schedule the task in windows

            //#if DEBUG
            //                _ => throw new NotImplementedException(nameof(RefreshInstalledProfile))
            //#else
            //				_ => false
            //#endif
            //            };
        }


        public static void PromptAndUninstallSelf(Action<bool> shutdown)
        {
            //    var choice = MessageBox.Show(
            //        "You are currently in the process of completly uninstalling geteduroam.\n" +
            //        (CertificateStore.AnyRootCaInstalledByUs()
            //             ? "This means uninstalling all the trusted root certificates installed by this application.\n\n"
            //             : "\n") +
            //        "Are you sure you want to continue?",
            //        caption: "geteduroam",
            //        MessageBoxButton.YesNo,
            //        MessageBoxImage.Warning);

            //    if (choice != MessageBoxResult.Yes)
            //    {
            //        MessageBox.Show(
            //            "geteduroam has not been uninstalled.",
            //            caption: "geteduroam",
            //            MessageBoxButton.OK,
            //            MessageBoxImage.Information);
            //        return;
            //    }

            //    Installer.ExitAndUninstallSelf(
            //        success =>
            //        {
            //            // we cannot show a normal message box on success,
            //            // since we've dispatched a job to delete the running binary at this point
            //            // but we can spawn a PowerShell that will show the success message
            //            if (success)
            //            {
            //                var extinguishMe = new ProcessStartInfo
            //                {
            //                    FileName = "mshta",
            //                    Arguments = "vbscript:Execute(\"msgbox \"\"The application and its configuration have been uninstalled\"\", 0, \"\"Uninstall geteduroam\"\":close\")",
            //                    WindowStyle = ProcessWindowStyle.Normal, // Shows a console in the taskbar, but it's hidden
            //                    CreateNoWindow = true,
            //                    WorkingDirectory = "C:\\"
            //                };
            //                Process.Start(extinguishMe);
            //            }
            //            else
            //            {
            //                MessageBox.Show(
            //                    "geteduroam is not yet uninstalled! The uninstallation was aborted.",
            //                    caption: "Uninstall geteduroam",
            //                    MessageBoxButton.OK,
            //                    MessageBoxImage.Error);
            //            }
            //            shutdown(success);
            //        },
            //        doDeleteSelf: true);
        }


        private static Version? AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly()
                               .GetName().Version;
            }
        }

        // TODO: can we populate with from AssemblyName? in general, all mentions of "eduroam" and "geteduroam" should be configurable
        public static readonly SelfInstaller Installer = new SelfInstaller(
            applicationIdentifier: "geteduroam",
            applicationMetadata: new SelfInstaller.ApplicationMeta()
            {
                DisplayName = "geteduroam", // [REQUIRED] ProductName
                Publisher = "SURF",         // [REQUIRED] Manufacturer
                Version = AssemblyVersion?.ToString() ?? "",
                VersionMajor = AssemblyVersion?.Major.ToString(CultureInfo.InvariantCulture) ?? "",
                VersionMinor = AssemblyVersion?.Minor.ToString(CultureInfo.InvariantCulture) ?? "",
                HelpLink = null!,            // ARPHELPLINK
                HelpTelephone = null!,       // ARPHELPTELEPHONE
                InstallSource = null!,       // SourceDir
                URLInfoAbout = null!,        // ARPURLINFOABOUT
                URLUpdateInfo = null!,       // ARPURLUPDATEINFO
                AuthorizedCDFPrefix = null!, // ARPAUTHORIZEDCDFPREFIX
                Comments =
                    null!, // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
                Contact =
                    null!, // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
                Language = null, // ProductLanguage
                Readme = null!, // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
                SettingsIdentifier =
                    null!, // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
                NoRepair = true,
                NoModify = true,
            });
    }
}
