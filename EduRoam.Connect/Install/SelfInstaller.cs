using EduRoam.Connect.Exceptions;

using IWshRuntimeLibrary;

using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace EduRoam.Connect.Install
{
    /// <summary>
    /// Because reinventing the wheel is fun.
    /// This is probably not achievable with the provided installer?
    /// </summary>
	public class SelfInstaller
    {
        private readonly string applicationIdentifier;
        private ApplicationMeta applicationMetadata;

        public SelfInstaller(
            string applicationIdentifier,
            ApplicationMeta applicationMetadata)
        {
            this.applicationIdentifier = applicationIdentifier
                ?? throw new ArgumentNullException(paramName: nameof(applicationIdentifier));

            applicationMetadata.SetRequired(this);
            applicationMetadata.Nullcheck();
            this.applicationMetadata = applicationMetadata;
        }

        private static AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();

        public static SelfInstaller DefaultInstance => new SelfInstaller(
            applicationIdentifier: "geteduroam",
            applicationMetadata: new ApplicationMeta()
            {
                DisplayName = "geteduroam",  // [REQUIRED] ProductName
                Publisher = "SURF",  // [REQUIRED] Manufacturer
                Version = AssemblyName.Version?.ToString() ?? "",
                VersionMajor = AssemblyName.Version?.Major.ToString(CultureInfo.InvariantCulture) ?? "",
                VersionMinor = AssemblyName.Version?.Minor.ToString(CultureInfo.InvariantCulture) ?? "",
                HelpLink = null!,  // ARPHELPLINK
                HelpTelephone = null!,  // ARPHELPTELEPHONE
                InstallSource = null!,  // SourceDir
                URLInfoAbout = null!,  // ARPURLINFOABOUT
                URLUpdateInfo = null!,  // ARPURLUPDATEINFO
                AuthorizedCDFPrefix = null!,  // ARPAUTHORIZEDCDFPREFIX
                Comments = null!,  // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
                Contact = null!,  // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
                Language = null,  // ProductLanguage
                Readme = null!,  // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
                SettingsIdentifier = null!,  // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
                NoRepair = true,
                NoModify = true,
            }
        );

        public struct ApplicationMeta
        {
            // See https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
            private string DisplayIcon;                       // [SET AUTOMATICALLY]
            public string DisplayName { get; set; } // ProductName
            private string DisplayVersion { get => Version; } // [SET AUTOMATICALLY] Derived from ProductVersion
            public string Publisher { get; set; } // Manufacturer
            public string Version { get; set; } // Derived from ProductVersion
            public string VersionMajor { get; set; } // Derived from ProductVersion
            public string VersionMinor { get; set; } // Derived from ProductVersion
            public string HelpLink { get; set; } // ARPHELPLINK
            public string HelpTelephone { get; set; } // ARPHELPTELEPHONE
            private string InstallDate;                       // [SET AUTOMATICALLY] The last time this product received service.
            private string InstallLocation;                   // [SET AUTOMATICALLY] ARPINSTALLLOCATION
            public string InstallSource { get; set; } // SourceDir
            public Uri URLInfoAbout { get; set; } // ARPURLINFOABOUT
            public Uri URLUpdateInfo { get; set; } // ARPURLUPDATEINFO
            public string AuthorizedCDFPrefix { get; set; } // ARPAUTHORIZEDCDFPREFIX
            public string Comments { get; set; } // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
            public string Contact { get; set; } // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
            public uint? Language { get; set; } // ProductLanguage
            private string ModifyPath;                        // [SET AUTOMATICALLY] "Determined and set by the Windows Installer."
            public string Readme { get; set; } // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
            private string UninstallString;                   // [SET AUTOMATICALLY] "Determined and set by Windows Installer."
            public string SettingsIdentifier { get; set; } // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
            public bool? NoRepair { get; set; } // REG_DWORD
            public bool? NoModify { get; set; } // REG_DWORD
            private uint? EstimatedSize;                     // [SET AUTOMATICALLY] REG_DWORD


            // for SelfInstaller to use:

            public void SetRequired(
                SelfInstaller installer)
            {
                _ = installer ?? throw new ArgumentNullException(paramName: nameof(installer));
                this.DisplayIcon = installer.InstallExePath;
                this.InstallLocation = installer.InstallDir;
                this.InstallDate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                this.UninstallString = installer.UninstallCommand;
                this.EstimatedSize = (uint)new FileInfo(ThisExePath).Length / 1024;
                this.ModifyPath = null;
                // TODO: SettingsIdentifier = ?
            }

            public void Write(
                Action<string, string> strWriter,
                Action<string, uint?> intWriter)
            {
                _ = strWriter ?? throw new ArgumentNullException(paramName: nameof(strWriter));
                _ = intWriter ?? throw new ArgumentNullException(paramName: nameof(intWriter));
                strWriter(nameof(DisplayIcon), DisplayIcon);
                strWriter(nameof(DisplayName), DisplayName);
                strWriter(nameof(DisplayVersion), DisplayVersion);
                strWriter(nameof(Publisher), Publisher);
                strWriter(nameof(Version), Version);
                strWriter(nameof(VersionMajor), VersionMajor);
                strWriter(nameof(VersionMinor), VersionMinor);
                strWriter(nameof(HelpLink), HelpLink);
                strWriter(nameof(HelpTelephone), HelpTelephone);
                strWriter(nameof(InstallDate), InstallDate);
                strWriter(nameof(InstallLocation), InstallLocation);
                strWriter(nameof(InstallSource), InstallSource);
                strWriter(nameof(URLInfoAbout), URLInfoAbout?.ToString());
                strWriter(nameof(URLUpdateInfo), URLUpdateInfo?.ToString());
                strWriter(nameof(AuthorizedCDFPrefix), AuthorizedCDFPrefix);
                strWriter(nameof(Comments), Comments);
                strWriter(nameof(Contact), Contact);
                intWriter(nameof(Language), Language);
                strWriter(nameof(ModifyPath), ModifyPath);
                strWriter(nameof(Readme), Readme);
                strWriter(nameof(UninstallString), UninstallString);
                strWriter(nameof(SettingsIdentifier), SettingsIdentifier);
                intWriter(nameof(NoRepair), NoRepair.HasValue ? (uint?)Convert.ToInt32(NoRepair, CultureInfo.InvariantCulture) : null);
                intWriter(nameof(NoModify), NoModify.HasValue ? (uint?)Convert.ToInt32(NoModify, CultureInfo.InvariantCulture) : null);
                intWriter(nameof(EstimatedSize), EstimatedSize);
            }

            public void Nullcheck()
            {
                _ = DisplayIcon ?? throw new NullReferenceException(nameof(DisplayIcon) + " can not be null");
                _ = DisplayName ?? throw new NullReferenceException(nameof(DisplayName) + " can not be null");
                _ = Publisher ?? throw new NullReferenceException(nameof(Publisher) + " can not be null");
                _ = Version ?? throw new NullReferenceException(nameof(Version) + " can not be null");
                _ = VersionMajor ?? throw new NullReferenceException(nameof(VersionMajor) + " can not be null");
                _ = VersionMinor ?? throw new NullReferenceException(nameof(VersionMinor) + " can not be null");
                _ = UninstallString ?? throw new NullReferenceException(nameof(UninstallString) + " can not be null");
                _ = EstimatedSize ?? throw new NullReferenceException(nameof(EstimatedSize) + " can not be null");
            }
        }


        // Shorthands

        public static string AppdataLocalDir
        { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }

        public static string StartmenuProgramsDir
        { get => Environment.GetFolderPath(Environment.SpecialFolder.Programs); }

        public static string ThisExePath
        { get => Process.GetCurrentProcess().MainModule.FileName; }

        public string InstallDir
        { get => AppdataLocalDir + Path.DirectorySeparatorChar + applicationIdentifier; }

        public string InstallExePath
        { get => InstallDir + Path.DirectorySeparatorChar + applicationIdentifier + ".exe"; }

        // TODO: add /Refresh as a property

        public string StartMinimizedCommand
        { get => InstallExePath + " /Background"; }

        public string UninstallCommand
        { get => InstallExePath + " /Uninstall"; }

        public string CloseCommand
        { get => InstallExePath + " /Close"; }

        public string StartMenuLnkPath
        { get => StartmenuProgramsDir + Path.DirectorySeparatorChar + applicationIdentifier + ".lnk"; }

        public string ScheduledTaskName
        { get => applicationIdentifier + " - Check for updated config"; }

        // Registry Namespaces
        private static string rnsRun
        { get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"; }
        private string rnsUninstall
        { get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + applicationIdentifier; }


        // Public interface

        /// <summary>
        /// If false, it is probably running from %HOME%/Downloads or something
        /// </summary>
        public bool IsInstalled
        { get => System.IO.File.Exists(InstallExePath); }

        /// <summary>
        /// If false, it is probably running from %HOME%/Downloads or something
        /// </summary>
        public bool IsRunningInInstallLocation
        { get => InstallExePath == ThisExePath; }

        public void EnsureIsInstalled()
        {
            if (IsRunningInInstallLocation) return; // TODO: some flow to update itself
            if (IsInstalled)
            {
                // TODO: assemblyversion instad of file date
                var d1 = System.IO.File.GetLastWriteTime(ThisExePath);
                var d2 = System.IO.File.GetLastWriteTime(InstallExePath);
                if (DateTime.Compare(d1, d2) <= 0)
                {
                    // TODO: console no work
                    Console.WriteLine(
                        "The date of the currently installed version " +
                        "is equal to or newer than this one.");
                    return;
                }
            }

            // TODO: user downloads new geteduroam -> runs the file -> single-instance running from install folder starts -> no update of binary

            InstallToUserLocal();
        }

        /// <summary>
        /// Installs the running EXE to %USER%/AppData/Local,
        /// registering it to the registry for the current user
        /// </summary>
        private void InstallToUserLocal()
        {
            // avoid uneccesary/illegal updates
            if (IsRunningInInstallLocation) // sanity check, should never happen
                throw new EduroamAppUserException("already installed", // TODO: use a more fitting exception?
                    "This application has already been installed. " +
                    "Installing it again won't have any effect.");

            // Create target install directory
            if (!Directory.Exists(InstallDir))
                Directory.CreateDirectory(InstallDir);

            // write executable, not retaining Zone.Identifier NTFS stream
            /*
   File.Copy(ThisExePath, InstallExePath, overwrite: true); // BAD: keeps NTFS streams which we don't want
   */
            //
            // Reading and writing manually works better, because then the resulting .exe can be openend
            // at startup or by the scheduler without the user getting "Are you sure you want to run this software?"
            var binaryExe = System.IO.File.ReadAllBytes(ThisExePath);
            System.IO.File.WriteAllBytes(InstallExePath, binaryExe);

            // Register the application in Windows
            applicationMetadata.Write(
                intWriter: (key, value) =>
                {
                    if (value == null) return; // ignore null values
                    Debug.WriteLine("Write int to {0}\\{1}: {2}", rnsUninstall, key, value);
                    Registry.SetValue(rnsUninstall, key, value, RegistryValueKind.DWord);
                },
                strWriter: (key, value) =>
                {
                    if (value == null) return; // ignore null values
                    Debug.WriteLine("Write str to {0}\\{1}: {2}", rnsUninstall, key, value);
                    Registry.SetValue(rnsUninstall, key, value);
                });

            // Add shortcut to start menu
            Debug.WriteLine("Create shortcut: " + StartMenuLnkPath);
            if (!System.IO.File.Exists(StartMenuLnkPath))
                System.IO.File.Delete(StartMenuLnkPath);
            var wshell = new WshShell();
            var lnk = wshell.CreateShortcut(StartMenuLnkPath) as IWshShortcut;
            lnk.TargetPath = InstallExePath;
            lnk.WorkingDirectory = InstallDir;
            lnk.Save();

            // Register scheduled task to check for updates
            Debug.WriteLine("Create scheduled task: " + ScheduledTaskName);
            using (var ts = new TaskService())
            {
                var task = ts.NewTask();
                task.Settings.AllowDemandStart = true;
                task.Settings.StartWhenAvailable = true; // run as soon as possible after a scheduled start is missed
                task.Settings.DisallowStartIfOnBatteries = false;

                if (applicationMetadata.Publisher != null)
                    task.RegistrationInfo.Author = applicationMetadata.Publisher;

                task.Actions.Add(new ExecAction(InstallExePath, arguments: "/Refresh"));

                /*
       task.Triggers.Add(new DailyTrigger(daysInterval: 3) { // every 3 days
           StartBoundary = DateTime.Today.AddHours(12) }); // around noon
       */

                // TODO: switch from the schedule below to the schedule above when certificate lifetime is extended for production

                // Every day, six times
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(0) });
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(4) });
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(8) });
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(12) });
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(16) });
                task.Triggers.Add(new DailyTrigger(daysInterval: 1)
                { StartBoundary = DateTime.Today.AddHours(20) });

                try
                {
                    ts.RootFolder.RegisterTaskDefinition(ScheduledTaskName, task);
                }
                catch (UnauthorizedAccessException)
                {
                    // TODO: we were not allowed to create the scheduled task
                }
            }
        }

        /// <summary>
        /// Uninstalls the program.
        /// </summary>
        /// <typeparam name="T">return value</typeparam>
        /// <param name="shutdownAction">a action which will shut down the application in the way you want, recieves true on successfull uninstall</param>
        /// <param name="doDeleteSelf">whether to schedule a deletion of InstallExePath</param>
        /// <returns>T</returns>
        public void ExitAndUninstallSelf(Action<bool> shutdownAction, bool doDeleteSelf = false)
        {
            _ = shutdownAction ?? throw new ArgumentNullException(paramName: nameof(shutdownAction));

            try
            {
                ConnectToEduroam.RemoveAllWLANProfiles();
                CertificateStore.UninstallAllInstalledCertificates(abortOnFail: true, omitRootCa: false);
            }
            catch (Exception)
            {
                shutdownAction(false);
                return;
            }
            LetsWifi.WipeTokens();
            PersistingStore.IdentityProvider = null;

            // Remove start menu link
            Debug.WriteLine("Delete file: " + this.StartMenuLnkPath);
            if (System.IO.File.Exists(this.StartMenuLnkPath)) System.IO.File.Delete(this.StartMenuLnkPath);

            // remove update task
            Debug.WriteLine("Delete scheduled task: " + this.ScheduledTaskName);
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(this.ScheduledTaskName,
                    exceptionOnNotExists: false);
            }

            // remove registry entries
            Debug.WriteLine("Delete registry value: " + rnsRun + "\\" + this.applicationIdentifier);
            using (var key = Registry.CurrentUser
                    .OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
                if (key?.GetValue(this.applicationIdentifier) != null)
                    key.DeleteValue(this.applicationIdentifier);
            Debug.WriteLine("Delete registry subkey: " + this.rnsUninstall); ;
            using (var key = Registry.CurrentUser
                    .OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", writable: true))
                if (key?.OpenSubKey(this.applicationIdentifier) != null)
                    key.DeleteSubKeyTree(this.applicationIdentifier); // TODO: for some reason this doesn't seem to work

            // Delete myself:
            if (System.IO.File.Exists(this.InstallExePath) && doDeleteSelf)
            {
                // this process delays 3 seconds then deletes the exe file
                var extinguishMe = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C choice /C Y /N /D Y /T 5 " +
                        "& Del " + ShellEscape(this.InstallExePath) +
                        "& Del /Q " + ShellEscape(this.InstallDir) +
                        "& rmdir " + ShellEscape(this.InstallDir),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    WorkingDirectory = "C:\\"
                };
                Process.Start(extinguishMe);
            }

            // Quit
            shutdownAction(true);
        }

        private static string ShellEscape(string arg)
        {
            return arg.Replace("%", "^%").Replace(" ", "^ ");
        }

    }
}
