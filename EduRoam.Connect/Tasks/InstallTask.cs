using EduRoam.Connect.Install;

namespace EduRoam.Connect.Tasks
{
    public class InstallTask
    {
        public static void Install()
        {
            SelfInstaller.DefaultInstance.EnsureIsInstalled();
        }
    }
}
