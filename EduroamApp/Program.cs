using ManagedNativeWifi;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EduroamApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
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



            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmParent());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
