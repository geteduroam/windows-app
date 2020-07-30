using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EduroamApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            /*
            * /
            var insti = EduroamConfigure.IdentityProviderDownloader.GetAllIdProviders()
                .Where(p => p.Country == "NL")
                .Where(p => p.Name == "eduroam Visitor Access (eVA)")
                .Select(p => p.Profiles.First())
                .First();
            var eapConfig = EduroamConfigure.EapConfig.FromXmlData(
                EduroamConfigure.IdentityProviderDownloader.GetEapConfigString(insti.Id)
            );
            foreach (var installer in EduroamConfigure.ConnectToEduroam.InstallEapConfig(eapConfig))
            {
                installer.InstallCertificates();
                installer.InstallProfile();
                EduroamConfigure.ConnectToEduroam.InstallUserProfile(
                    "trololo@edu.nl", "hunter2", installer.AuthMethod);
                break;
            }
            var task = EduroamConfigure.ConnectToEduroam.TryToConnect();
            //task.RunSynchronously();
            Console.Write("TryToConnect: ");
            Console.WriteLine(task.Result);
            Console.WriteLine("NetworkPacks:");
            NativeWifi.EnumerateAvailableNetworks().ToList().ForEach(pack =>
            {
                Console.Write(" - ");
                Console.Write(pack.Ssid);
                Console.Write(" - ");
                Console.Write(pack.ProfileName ?? "no profile");
                Console.Write(" @ ");
                Console.Write(pack.Interface.Id);
                Console.Write("-");
                Console.Write(pack.Interface.Description);
                Console.WriteLine();
            });

            Console.WriteLine();
            Console.WriteLine("Profiles:");
            NativeWifi.EnumerateProfiles().ToList().ForEach(profile =>
            {
                Console.Write(" - ");
                Console.Write(profile.Name);
                Console.Write(" @ ");
                Console.Write(profile.Interface.Id);
                Console.Write("-");
                Console.Write(profile.Interface.Description);
                Console.WriteLine();
            });
            return;
            /*
            */


            RunGUI(args);
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
