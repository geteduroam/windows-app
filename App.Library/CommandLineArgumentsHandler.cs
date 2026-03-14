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
        public static async Task<bool> PreGuiCommandLineArgs(string[] args)
        {
            if (args.Length == 0 || (args.Length == 2 && "-ToastActivated -Embedding".Equals(String.Join(" ", args)))) {
                // When the Toast button is pressed, we get these arguments
                // If we actually want to have different kinds of Toast buttons,
                // we should do something smarter here, but for now the button just launches the app.

                return false; // continue to app
            }
            // We use / flags, and Toast uses -ToastActivated -Embedding
            // So probably best to not assume that an argument starting with - is a file name
            if (args[0][0] != '/' && args[0][0] != '-')
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
                case "/refresh": return await RefreshCertificate(force, verbose ?? false);
                case "/certificate-notify": CertificateToast(force || (verbose ?? false)); return true;
                case "/close": return true;
                case "/help":
                case null:
                default:
                    ShowHelpText();
                    return true;
            }
        }

        private async static Task<bool> RefreshCertificate(bool force, bool verbose)
        {
            var expiration = await RefreshTask.RefreshAsync(force: force);

            return !verbose || string.IsNullOrWhiteSpace(expiration);
        }

        private static void CertificateToast(bool verbose)
        {
            var st = new StatusTask();
            var gst = st.GetStatus();
            var diffDate = (gst.ExpirationDate - DateTime.Now).Value.Days;

            if (verbose || diffDate <= Settings.Settings.DaysLeftForNotification)
            {
                new ToastContentBuilder()
                    .AddText(string.Format(Resources.CheckCertificateToastP1, Settings.Settings.ApplicationName))
                    .AddText(string.Format(Resources.CheckCertificateToastP2, diffDate))
                    .AddButton(new ToastButton() { ActivationType = ToastActivationType.Foreground }
                        .SetContent(Resources.CheckCertificateToastButton)                        
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
                caption: Settings.Settings.ApplicationName);

        private static string HelpTextIndent(string s) =>
            "            " + s.Replace("\n", "\n            ");
    }
}
