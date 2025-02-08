using IWshRuntimeLibrary;

using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

using Semver;

using System;
using System.Diagnostics;
using System.IO;
using EduRoam.Connect.Tasks;

namespace App.Library.Install
{
    /// <summary>
    /// Because reinventing the wheel is fun.
    /// This is probably not achievable with the provided installer?
    /// </summary>
	public partial class SelfInstaller
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

        public static SelfInstaller DefaultInstance => new(
            applicationIdentifier: Settings.Settings.ApplicationIdentifier,
            applicationMetadata: new ApplicationMeta()
            {
                DisplayName = Settings.Settings.ApplicationIdentifier,  // [REQUIRED] ProductName
                Publisher = "SURF",  // [REQUIRED] Manufacturer
                Version = RunningVersion.ToString(),
                VersionMajor = RunningVersion.Major.ToString(),
                VersionMinor = RunningVersion.Minor.ToString(),
                HelpLink = Settings.Settings.HelpUrl,  // ARPHELPLINK
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

        // Shorthands

        public static string AppdataLocalDir
        {
            get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public static string ProgramFilesDir
        {
            get => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public static string UserStartmenuProgramsDir
        {
            get => Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        }

        public static string RunningExePath
        {
            get => Process.GetCurrentProcess().MainModule!.FileName!;
        }

        public static string UserTemporaryDir
        {
            get => AppdataLocalDir + Path.DirectorySeparatorChar + "Temp";
        }

        public string UserInstallDir
        {
            get => AppdataLocalDir + Path.DirectorySeparatorChar + this.applicationIdentifier;
        }
        public string GlobalInstallDir
        {
            get => ProgramFilesDir + Path.DirectorySeparatorChar + this.applicationIdentifier;
        }

        public string UserInstallExePath
        {
            get => this.UserInstallDir + Path.DirectorySeparatorChar + this.applicationIdentifier + ".exe";
        }

        public string GlobalInstallExePath
        {
            get => this.GlobalInstallDir + Path.DirectorySeparatorChar + this.applicationIdentifier + ".exe";
        }

        public string UninstallDir
        {
            get => UserTemporaryDir + Path.DirectorySeparatorChar + this.applicationIdentifier + ".delete";
        }

        // TODO: add /Refresh as a property

        public string UserUninstallCommand
        {
            get => this.UserInstallExePath + " /Uninstall";
        }

        public string UserStartMenuLnkPath
        {
            get => UserStartmenuProgramsDir + Path.DirectorySeparatorChar + this.applicationIdentifier + ".lnk";
        }

        public string ScheduledTaskName
        {
            get => this.applicationIdentifier + " - Check for updated config";
        }

        // Public interface

        /// <summary>
        /// If false, it is probably running from %HOME%/Downloads or something
        /// </summary>
        public bool IsUserInstalled
        {
            get => System.IO.File.Exists(this.UserInstallExePath);
        }
        public bool IsGloballyInstalled
        {
            get=> System.IO.File.Exists(this.GlobalInstallExePath);
        }
        public bool IsInstalled
        {
            get => this.IsUserInstalled || this.IsGloballyInstalled;
        }
        public string? InstalledExePath
        {
            get => this.IsGloballyInstalled ? this.GlobalInstallExePath : this.IsUserInstalled ? this.UserInstallExePath : null;
        }
        public static SemVersion? RunningVersion { 
            get => _getFileVersion(RunningExePath); 
        }

        /// <summary>
        /// If false, it is probably running from %HOME%/Downloads or something
        /// </summary>
        public bool IsRunningInUserInstallLocation
        {
            get => this.UserInstallExePath == RunningExePath;
        }

        public bool IsRunningInGlobalInstallLocation
        {
            get => this.GlobalInstallExePath == RunningExePath;
        }
        public void UpdateWithFile(string path)
        {
            if(this.IsUserInstalled)
            {
                if (this.CanUpdateUserInstalled(this.GetFileVersion(path)))
                {
                    this.InstallToUserLocal(path, true);
                    this.SetUserInstalledState(true);
                }
            }
            else
            {
                this.EnsureIsInstalled(path);
            }
        }
        public void EnsureIsInstalled(string? path = null)
        {
            if (!this.IsGloballyInstalled) {
                if (path == null || this.CanUpdateUserInstalled(this.GetFileVersion(path))
                    || (path == null && !this.IsRunningInUserInstallLocation && this.IsRunningNewerThanUserInstalled()))
                {
                    this.InstallToUserLocal(path);
                    this.SetUserInstalledState(true);
                }
            }
            this.SetFileAssociationRegistered(true);
            this.SetStartMenuEntry(true);
            this.SetScheduledTask(true);
        }
        /// <summary>
        /// Uninstalls the program, this will also remove all configuration unless the application is also globally installed
        /// </summary>
        /// <param name="shutdownAction">a action which will shut down the application in the way you want, recieves true on successfull uninstall</param>
        /// <returns>Whether removing succeeded, same value as was passed to shutdownAction</returns>
        public bool EnsureIsUserUninstalled()
        {
            if (!this.IsGloballyInstalled) try
            {
                RemoveWiFiConfigurationTask.Remove(omitRootCa: false);
            }
            catch (Exception)
            {
                return false;
            }

            this.CleanupRegistry();

            this.SetStartMenuEntry(false);
            this.SetScheduledTask(false); // TODO fix scheduled task for global installation if global installed
            this.SetFileAssociationRegistered(false);

            if (this.RemoveFromUserLocal())
            {
                this.SetUserInstalledState(false);
                return true;
            }
            return false;
        }

        private void CleanupRegistry()
        {
            // remove autorun registry entries (we don't make these, but a very old version of the app did, so remove those)
            Debug.WriteLine("Delete registry value: Software\\Microsoft\\Windows\\CurrentVersion\\Run\\" + this.applicationIdentifier);
            using (var key = Registry.CurrentUser
                    .OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
            {
                key.DeleteValue(this.applicationIdentifier, false);
            }
        }

        /// <summary>
        /// Installs the running EXE to %USER%/AppData/Local,
        /// registering it to the registry for the current user
        /// </summary>
        /// <param name="path">Path of the file to install, null for the currently running file</param>
        /// <param name="tryMove">Normally the installation is done by copying the file, removing all metadata, but with this setting the file is moved instead; if moving fails the normal copy method is used regardless</param>
        private void InstallToUserLocal(string? path, bool tryMove = false)
        {
            // Create target install directory
            if (!Directory.Exists(this.UserInstallDir))
            {
                Directory.CreateDirectory(this.UserInstallDir);
            }

            if (System.IO.File.Exists(this.UserInstallExePath))
            {
                // Version 4.2.0 and 4.2.1 will call us with /install, while running from user install exe path
                // and we're on a temporary location, so we cannot be sure we can remove the existing file.
                // But we can try to move it somewhere else.

                SemVersion version;
                try
                {
                    version = this.GetFileVersion(this.UserInstallExePath);
                }
                catch (Exception _)
                {
                    version = null;
                }

                string tempPath = null;
                do
                {
                    tempPath = version == null
                        ? this.UninstallDir + Path.DirectorySeparatorChar + this.applicationIdentifier + "_exe-" + Guid.NewGuid() + ".delete"
                        : this.UserInstallDir + Path.DirectorySeparatorChar + this.applicationIdentifier + "-" + version + ".exe";

                    if (System.IO.File.Exists(tempPath))
                    {
                        try
                        {
                            System.IO.File.Delete(tempPath);
                        }
                        catch (IOException _) { }
                    }
                    if (version != null && System.IO.File.Exists(tempPath))
                    {
                        version = null;
                    }
                } while (System.IO.File.Exists(tempPath));

                RemoveOrRenameFile(this.UserInstallExePath, tempPath, version == null ? this.UninstallDir : null);
            }

            if (tryMove) try
            {
                System.IO.File.Move(path ?? RunningExePath, this.UserInstallExePath);
                return;
            } catch (UnauthorizedAccessException) { }

            // write executable, not retaining Zone.Identifier NTFS stream
            /*
            File.Copy(ThisExePath, InstallExePath, overwrite: true); // BAD: keeps NTFS streams which we don't want
            */
            //
            // Reading and writing manually works better, because then the resulting .exe can be openend
            // at startup or by the scheduler without the user getting "Are you sure you want to run this software?"
            var binaryExe = System.IO.File.ReadAllBytes(path ?? RunningExePath);
            System.IO.File.WriteAllBytes(this.UserInstallExePath, binaryExe);
        }
        /// <summary>
        /// Removes the installed executable when its running from installed location
        /// </summary>
        public bool RemoveFromUserLocal()
        {
            var uninstallTempFile = this.UninstallDir + Path.DirectorySeparatorChar + this.applicationIdentifier + "_exe-" + Guid.NewGuid() + ".delete";
            if (!RemoveOrRenameFile(this.UserInstallExePath, uninstallTempFile, this.UninstallDir)) return false;

            // Delete myself after 15 seconds:
            if (System.IO.File.Exists(uninstallTempFile))
            {
                // this process delays 15 seconds then deletes the moved exe file
                var extinguishMe = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C timeout 15" +
                        "& Del " + ShellEscape(uninstallTempFile) +
                        "& Del /Q " + ShellEscape(this.UserInstallDir) +
                        "& Del /Q " + ShellEscape(this.UninstallDir) +
                        "& rmdir " + ShellEscape(this.UserInstallDir) +
                        "& rmdir " + ShellEscape(this.UninstallDir),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    WorkingDirectory = "C:\\"
                };
                Process.Start(extinguishMe);
            }
            return true;
        }
        private static string ShellEscape(string arg)
        {
            return arg.Replace("^", "^^").Replace("&", "^&").Replace("<", "^<").Replace(">", "^>").Replace("|", "^|").Replace("%", "%%").Replace(" ", "^ ");
        }
        private static bool TryDelete(string path)
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch (IOException e) { Debug.WriteLine(e); }
            catch (UnauthorizedAccessException e) { Debug.WriteLine(e); }
            return !System.IO.File.Exists(path);
        }
        private static bool RemoveOrRenameFile(string path, string moveTarget, string? moveDirectory = null)
        {
            try
            {
                // Maybe we can just remove it anyway, since it's not us
                System.IO.File.Delete(path);
                return true;
            }
            catch (UnauthorizedAccessException _)
            {
                // We were not allowed to remove ourselves, probably because the application is still running
                // We move the file to the Temp directory
                try
                {
                    if (moveDirectory != null)
                        Directory.CreateDirectory(moveDirectory);
                    System.IO.File.Move(path, moveTarget);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
        #region Installers/uninstallers for different Windows components

        public void SetUserInstalledState(bool installed)
        {
            var installedAppsRegKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
            var applicationRegKey = installedAppsRegKey.CreateSubKey(this.applicationIdentifier);
            {
                if (installed)
                {
                    this.applicationMetadata.Write(
                        intWriter: (key, value) =>
                        {
                            if (value != null)
                            applicationRegKey.SetValue(key, value, RegistryValueKind.DWord);
                        },
                        strWriter: (key, value) =>
                        {
                            if (value != null)
                            applicationRegKey.SetValue(key, value);
                        }
                    );
                }
                else
                {
                    if (applicationRegKey != null)
                        installedAppsRegKey.DeleteSubKeyTree(this.applicationIdentifier);
                }
            }
        }
        public void SetFileAssociationRegistered(bool registered)
        {
            const string REGISTRY_KEY = "Software\\Classes\\.eap-config";

            if (registered)
            {
                // Add file association
                var fileRegEapConfig = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY);
                fileRegEapConfig.CreateSubKey("shell\\open\\command").SetValue(null, $"{this.UserInstallExePath} \"%1\"");
                // TODO: Use a document icon instead of the same icon as the exe file
                fileRegEapConfig.CreateSubKey("DefaultIcon").SetValue(null, $"{this.UserInstallExePath}");
                fileRegEapConfig.Close();
            }
            else
            {
                // remove file association
                var fileRegEapConfig = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY);
                if (fileRegEapConfig != null)
                {
                    var subkey = fileRegEapConfig.OpenSubKey("shell\\open\\command");
                    // If .eap-config is still ours, remove the file association
                    if (string.Equals(subkey?.GetValue(null), $"{this.UserInstallExePath} \"%1\""))
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(REGISTRY_KEY, false);
                    }
                    fileRegEapConfig.Close();
                }
            }
        }
        public void SetStartMenuEntry(bool present)
        {
            if (present)
            {
                // Add shortcut to start menu
                Debug.WriteLine("Create shortcut: " + this.UserStartMenuLnkPath);
                var wshell = new WshShell();
                var lnk = wshell.CreateShortcut(this.UserStartMenuLnkPath) as IWshShortcut;
                if (lnk != null)
                {
                    lnk.TargetPath = this.UserInstallExePath;
                    lnk.WorkingDirectory = this.UserInstallDir;
                    lnk.Save();
                }
            } else
            {
                // Remove start menu link
                Debug.WriteLine("Delete file: " + this.UserStartMenuLnkPath);
                if (System.IO.File.Exists(this.UserStartMenuLnkPath))
                {
                    System.IO.File.Delete(this.UserStartMenuLnkPath);
                }
            }
        }
        public void SetScheduledTask(bool installed)
        {
            using var ts = new TaskService();
            if (installed)
            {
                if (this.InstalledExePath == null)
                {
                    Debug.WriteLine("Unable to create scheduled task unless application is installed");
                    return;
                }

                // Register scheduled task to check for updates
                Debug.WriteLine("Create scheduled task: " + this.ScheduledTaskName);
                var task = ts.NewTask();
                task.Settings.AllowDemandStart = true;
                task.Settings.StartWhenAvailable = true; // run as soon as possible after a scheduled start is missed
                task.Settings.DisallowStartIfOnBatteries = false;

                if (this.applicationMetadata.Publisher != null)
                {
                    task.RegistrationInfo.Author = this.applicationMetadata.Publisher;
                }

                task.Actions.Add(new ExecAction(this.InstalledExePath, arguments: "/refresh"));

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

                /*
                task.Triggers.Add(new DailyTrigger(daysInterval: 3) { // every 3 days
                StartBoundary = DateTime.Today.AddHours(12) }); // around noon
                */

                try
                {
                    ts.RootFolder.RegisterTaskDefinition(this.ScheduledTaskName, task);
                }
                catch (UnauthorizedAccessException)
                {
                    // TODO: we were not allowed to create the scheduled task
                }

                // create a second scheduled task to make toast
                using var toastService = new TaskService();
                var toastTask = toastService.NewTask();
                toastTask.Settings.AllowDemandStart = true;
                toastTask.Settings.StartWhenAvailable = true; // run as soon as possible after a scheduled start is missed
                toastTask.Settings.DisallowStartIfOnBatteries = false;

                if (this.applicationMetadata.Publisher != null)
                {
                    toastTask.RegistrationInfo.Author = this.applicationMetadata.Publisher;
                }

                toastTask.Actions.Add(new ExecAction(this.InstalledExePath, arguments: "/check-certificate"));

                toastTask.Triggers.Add(new DailyTrigger(1)
                {
                    StartBoundary = DateTime.Today.AddHours(12)
                });

                try
                {
                    toastService.RootFolder.RegisterTaskDefinition(this.ScheduledTaskName + " - Toast", toastTask);
                }
                catch (UnauthorizedAccessException)
                {
                    // TODO: we were not allowed to create the scheduled task
                }
            }
            else
            {
                // remove update task
                Debug.WriteLine("Delete scheduled task: " + this.ScheduledTaskName);
                ts.RootFolder.DeleteTask(this.ScheduledTaskName,
                    exceptionOnNotExists: false);
                ts.RootFolder.DeleteTask(this.ScheduledTaskName + " - Toast",
                    exceptionOnNotExists: false);
            }
        }
        #endregion

        #region AutoInstaller and UpdateChecker helper functions
        /// <summary>
        /// For AutoInstaller, location is Install Path
        /// </summary>
        public bool IsRunningNewerThanUserInstalled()
        {
            return this.CanUpdateUserInstalled(RunningVersion);
        }
        /// <summary>
        /// For UpdateChecker, location is Current path
        /// </summary>
        /// <param name="latestVersion"></param>
        /// <returns></returns>
        public bool CanUpdateRunning(SemVersion latestVersion)
        {
            return !this.IsRunningInGlobalInstallLocation && CanUpdate(RunningVersion, latestVersion);
        }
        public static bool CanUpdate(SemVersion current, SemVersion latest)
        {
            return SemVersion.ComparePrecedence(current, latest) == -1;
        }
        /// <summary>
        /// For UpdateChecker, location is installed path
        /// </summary>
        /// <param name="latestVersion"></param>
        /// <returns></returns>
        public bool CanUpdateUserInstalled(SemVersion latestVersion)
        {
            if (this.IsRunningInGlobalInstallLocation) return false;
            var installedVersion = this.GetFileVersion(this.UserInstallExePath);
            return CanUpdate(installedVersion, latestVersion);
        }

        public void StartApplicationFromInstallLocation()
        {
            Process.Start(this.IsGloballyInstalled ? this.GlobalInstallExePath : this.UserInstallExePath);
        }

        public SemVersion? GetFileVersion(string path)
        {
            return this.IsSameApplication(path) ? _getFileVersion(path) : null;
        }
        private static SemVersion? _getFileVersion(string path)
        {
            try
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(path);
                // Cannot use SemVersion.Parse() because fileVersion.Version also contains the FilePrivatePart number,
                // which SemVersion.Parse will throw an exception for.
                return new SemVersion(fileVersion.FileMajorPart, fileVersion.FileMinorPart, fileVersion.FileBuildPart);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        /// <summary>
        /// Determine if the path refers to the same application (but maybe a different version or arch)
        /// This is a simple check to make sure that we're only replacing geteduroam with geteduroam, and getgovroam with getgovroam.
        /// </summary>
        /// <param name="path">Path of the file to test</param>
        /// <returns>Whether it's the same application</returns>
        private bool IsSameApplication(string path)
        {
            try
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(path);
                if (fileVersion == null) return false;
                return fileVersion.ProductName == Settings.Settings.ApplicationIdentifier;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public SemVersion GetRunningVersion() => RunningVersion;
        #endregion
    }

}
