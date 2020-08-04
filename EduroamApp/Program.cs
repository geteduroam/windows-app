using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

// TODO: support program to start hidden with tray icon

namespace EduroamApp
{
    static class Program
    {
        static readonly SelfInstaller GetEduroamInstaller = new SelfInstaller(
            applicationIdentifier: "GetEduroam", // we could use Application.ProductName
            applicationMetadata: new SelfInstaller.ApplicationMeta()
            {
                DisplayName         = "GetEduroam",  // [REQUIRED] ProductName
                Publisher           = "Uninett",  // [REQUIRED] Manufacturer
                HelpLink            = null,  // ARPHELPLINK
                HelpTelephone       = null,  // ARPHELPTELEPHONE
                InstallSource       = null,  // SourceDir
                URLInfoAbout        = null,  // ARPURLINFOABOUT
                URLUpdateInfo       = null,  // ARPURLUPDATEINFO
                AuthorizedCDFPrefix = null,  // ARPAUTHORIZEDCDFPREFIX
                Comments            = null,  // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
                Contact             = null,  // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
                Language            = null,  // ProductLanguage
                Readme              = null,  // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
                SettingsIdentifier  = null,  // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
                NoRepair            = null,  // [REQUIRED] REG_DWORD
                NoModify            = null,  // [REQUIRED] REG_DWORD
            }
        );


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // TODO:
            //if (args.Contains("/InstallEapConfig"))

            IEnumerable<string> argsLower() =>
                args.Select(p => p.ToLowerInvariant());


            if (argsLower().Contains("/?")
                || argsLower().Contains("/help"))
            {
                Console.WriteLine("Supported arguments:");
                foreach ((string cmd, string desc) in new (string, string)[] {
                        ("/?",
                            "This help text"),
                        ("/Help",
                            "This help text"),
                        ("/Refresh",
                            "If installed with a refresh token, check for a refresh"),
                        ("/Background",
                            "Will start minimized to the tray."),
                        ("/Install",
                            "Will install app to %USER%/AppData/Local"),
                        ("/Uninstall",
                            "Will uninstall the program from %USER%/AppData/Local"),
                        ("/Close",
                            "Closes the single-instance running for this application"),
                    }) Console.WriteLine("\t{0, -24} {1}", cmd, desc.Replace("\n", "\n\t\t"));
            }
            else if (argsLower().Contains("/install"))
            {
                GetEduroamInstaller.InstallToUserLocal();
            }
            else if (argsLower().Contains("/uninstall"))
            {
                // TODO: confirmation dialog
                GetEduroamInstaller.ExitAndUninstallSelf();
            }
            else
            {
                // if there are supported NICs to configure
                if (!EduroamConfigure.EduroamNetwork.GetAll(null).Any())
                {
                    MessageBox.Show(
                        "No supported network interface was found on this computer,\n" +
                        "we are therefore unable to configure eduroam.\n\n" +
                        "Please go to Control Panel -> Network and Internet -> Network Connections to make sure that it is enabled.\n",
                        caption: Application.ProductName + " - " + Application.ProductVersion,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // handles the rest
                RunGUI(args);
            }
        }

        static void RunGUI(string[] args)
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /**/

            // Single-instance launch:
            var sim = new SingleInstanceManager();
            sim.Run(args);

            /** /

            // Non single-instance launch:
            Application.Run(new frmParent());

            /**/
        }


        // from https://www.red-gate.com/simple-talk/dotnet/net-framework/creating-tray-applications-in-net-a-practical-guide/#seventeenth
        class SingleInstanceManager : WindowsFormsApplicationBase
        {
            public SingleInstanceManager()
            {
                IsSingleInstance = true;
                //EnableVisualStyles = true; // Doesn't seem to do anything
                //ShutdownStyle = Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses; // TODO: needed?
                MainForm = new frmParent();
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                //base.OnStartupNextInstance(eventArgs);
                if (MainForm.WindowState == FormWindowState.Minimized)
                    MainForm.WindowState = FormWindowState.Normal;
                MainForm.Activate();
                // eventArgs.CommandLine.ToArray()
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
