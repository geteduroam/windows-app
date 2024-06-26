using EduRoam.Connect.Tasks;
using EduRoam.Localization;

using Microsoft.Toolkit.Uwp.Notifications;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                InstallTask.Install();

                return true;
            }

            if (contains("/uninstall"))
            {
                UninstallTask.Uninstall(_ => { Environment.Exit(0); });

                return true;
            }

            if (contains("/refresh")
                || contains("/force-refresh")
                || contains("/refresh-force"))
            {
                Task.Run(async () => { await RefreshTask.RefreshAsync(force: contains("/refresh-force")); });

                return true;
            }

            if(contains("/check-certificate"))
            {
                var st = new StatusTask();
                var gst = st.GetStatus();
                var diffDate = (gst.ExpirationDate - DateTime.Now).Value.Days;

                if (diffDate <= Settings.Settings.DaysLeftForNotification)
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
                        Resources.AppCommandsHelp,
                    }),
                caption: Assembly.GetEntryAssembly()!.GetName().Name);
    }
}
