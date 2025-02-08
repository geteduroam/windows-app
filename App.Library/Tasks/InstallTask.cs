using App.Library.Install;

namespace App.Library.Tasks
{
    public class InstallTask
    {
        public static void Install()
        {
            SelfInstaller.DefaultInstance.EnsureIsInstalled();
        }
    }
}
