using App.Library.Tasks;

using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using Microsoft.Toolkit.Uwp.Notifications;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            if (args.Length == 0) {
                return false; // continue to app
            }
            if (args[0][0] != '/')
            {
                Settings.Settings.EapConfigFileLocation = args[0];
                return false; // continue to app
            }

            var force = false;
            Boolean? verbose = null;
            string action = null;

            for(var i=0;i<args.Length;i++) switch (args[i].ToLowerInvariant())
            {
                case "/force":
                    force = true;
                    break;

                case "/silent":
                    verbose = false;
                    break;

                case "/verbose":
                    verbose = true;
                    break;

                case "/install":
                case "/uninstall":
                case "/certificate-notify":
                case "/close":
                case "/refresh":
                case "/help":
                    action = action == null ? args[i].ToLowerInvariant() : "/help";
                    break;

                case "/force-refresh":
                case "/refresh-force":
                    force = true;
                    action = action == null ? "/refresh" : "/help";
                    break;

                case "/check-certificate":
                    action = action == null ? "/certificate-notify" : "/help";
                    break;

                case "/background":
                    action = action == null ? "/close" : "/help";
                    break;

                case "/?":
                default:
                    action = "/help";
                    break;
            }

            // Prevent /force on actions that don't support it
            if (force) switch (action)
            {
                case "/install":
                case "/uninstall":
                case null:
                    action = "/help";
                    break;
            }

            switch (action)
            {
                case "/install": InstallTask.Install(verbose ?? false); return true;
                case "/uninstall": UninstallTask.Uninstall(verbose ?? true); return true;
                case "/refresh": RefreshCertificate(force, verbose ?? false); return true;
                case "/certificate-notify": CertificateToast(force || (verbose ?? false)); return true;
                case "/close": return true;
                case "/help":
                case null:
                default:
                    ShowHelpText();
                    return true;
            }
        }

        private static void RefreshCertificate(bool force, bool verbose)
        {
            var expiration = Task.Run(async () => { return await RefreshTask.RefreshAsync(force: force); });
            expiration.RunSynchronously();

            if (verbose) {
                MessageBox.Show(string.IsNullOrWhiteSpace(expiration.Result) ? expiration.Result : "The certificate was not renewed");
            }
        }

        private static void CertificateToast(bool verbose)
        {
            var st = new StatusTask();
            var gst = st.GetStatus();
            var diffDate = (gst.ExpirationDate - DateTime.Now).Value.Days;

            if (verbose || diffDate <= Settings.Settings.DaysLeftForNotification)
            {
                new ToastContentBuilder()
                    .AddText(string.Format(Resources.CheckCertificateToastP1, Settings.Settings.ApplicationIdentifier))
                    .AddText(string.Format(Resources.CheckCertificateToastP2, diffDate))
                    .AddButton(new ToastButton()
                        .SetContent(Resources.CheckCertificateToastButton)
                        .SetBackgroundActivation()
                     )
                    .Show();
            }
        }

        private static void ShowHelpText() =>
            MessageBox.Show(
                string.Join(
                    "\r\n",
                    new List<string>
                    {
                        Resources.AppCommandLineHelpFlagsTitle,
                        string.Empty,
                        "      /help, /? :",
                        HelpTextIndent(Resources.AppCommandLineHelpFlagsHelp),
                        "      /install :",
                        HelpTextIndent(Resources.AppCommandLineHelpFlagsInstall),
                        "      /uninstall :",
                        HelpTextIndent(Resources.AppCommandLineHelpFlagsUninstall),
                        "      /refresh :",
                        HelpTextIndent(Resources.AppCommandLineHelpFlagsRefresh),
                    }),
                caption: Settings.Settings.ApplicationIdentifier);

        private static string HelpTextIndent(string s) =>
            "            " + s.Replace("\n", "\n            ");
    }
}
