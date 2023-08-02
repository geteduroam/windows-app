using EduRoam.Connect.Install;

namespace EduRoam.Connect.Tasks
{
    public class InstallTask
    {
        public void Install()
        {
            SelfInstaller.DefaultInstance.EnsureIsInstalled();
        }
    }
}
