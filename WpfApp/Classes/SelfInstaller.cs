using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;


namespace WpfApp
{
	/// <summary>
	/// Because reinventing the wheel is fun.
	/// This is probably not achievable with the provided installer?
	/// </summary>
	public class SelfInstaller
	{
		private readonly string ApplicationIdentifier;
		private ApplicationMeta ApplicationMetadata;

		public SelfInstaller(
			string applicationIdentifier,
			ApplicationMeta applicationMetadata)
		{
			ApplicationIdentifier = applicationIdentifier
				?? throw new ArgumentNullException(paramName: nameof(applicationIdentifier));

			applicationMetadata.SetRequired(this);
			applicationMetadata.Nullcheck();
			ApplicationMetadata = applicationMetadata;
		}

		public struct ApplicationMeta
		{
			// See https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
			private string DisplayIcon;                       // [SET AUTOMATICALLY]
			public  string DisplayName          { get; set; } // ProductName
			private string DisplayVersion       { get => Version; } // [SET AUTOMATICALLY] Derived from ProductVersion
			public  string Publisher            { get; set; } // Manufacturer
			public  string Version              { get; set; } // Derived from ProductVersion
			public  string VersionMajor         { get; set; } // Derived from ProductVersion
			public  string VersionMinor         { get; set; } // Derived from ProductVersion
			public  string HelpLink             { get; set; } // ARPHELPLINK
			public  string HelpTelephone        { get; set; } // ARPHELPTELEPHONE
			private string InstallDate;                       // [SET AUTOMATICALLY] The last time this product received service.
			private string InstallLocation;                   // [SET AUTOMATICALLY] ARPINSTALLLOCATION
			public  string InstallSource        { get; set; } // SourceDir
			public  Uri URLInfoAbout            { get; set; } // ARPURLINFOABOUT
			public  Uri URLUpdateInfo           { get; set; } // ARPURLUPDATEINFO
			public  string AuthorizedCDFPrefix  { get; set; } // ARPAUTHORIZEDCDFPREFIX
			public  string Comments             { get; set; } // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
			public  string Contact              { get; set; } // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
			public  uint?  Language             { get; set; } // ProductLanguage
			private string ModifyPath;                        // [SET AUTOMATICALLY] "Determined and set by the Windows Installer."
			public  string Readme               { get; set; } // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
			private string UninstallString;                   // [SET AUTOMATICALLY] "Determined and set by Windows Installer."
			public  string SettingsIdentifier   { get; set; } // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
			public  bool?  NoRepair             { get; set; } // REG_DWORD
			public  bool?  NoModify             { get; set; } // REG_DWORD
			private uint?  EstimatedSize;                     // [SET AUTOMATICALLY] REG_DWORD


			// for SelfInstaller to use:

			public void SetRequired(
				SelfInstaller installer)
			{
				_ = installer ?? throw new ArgumentNullException(paramName: nameof(installer));
				DisplayIcon     = installer.InstallExePath;
				InstallLocation = installer.InstallDir;
				InstallDate     = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				UninstallString = installer.UninstallCommand;
				EstimatedSize   = (uint)new FileInfo(SelfInstaller.ThisExePath).Length / 1024;
				ModifyPath      = null;
				// TODO: SettingsIdentifier = ?
			}

			public void Write(
				Action<string, string> strWriter,
				Action<string, uint?> intWriter)
			{
				_ = strWriter ?? throw new ArgumentNullException(paramName: nameof(strWriter));
				_ = intWriter ?? throw new ArgumentNullException(paramName: nameof(intWriter));
				strWriter(nameof(DisplayIcon),         DisplayIcon);
				strWriter(nameof(DisplayName),         DisplayName);
				strWriter(nameof(DisplayVersion),      DisplayVersion);
				strWriter(nameof(Publisher),           Publisher);
				strWriter(nameof(Version),             Version);
				strWriter(nameof(VersionMajor),        VersionMajor);
				strWriter(nameof(VersionMinor),        VersionMinor);
				strWriter(nameof(HelpLink),            HelpLink);
				strWriter(nameof(HelpTelephone),       HelpTelephone);
				strWriter(nameof(InstallDate),         InstallDate);
				strWriter(nameof(InstallLocation),     InstallLocation);
				strWriter(nameof(InstallSource),       InstallSource);
				strWriter(nameof(URLInfoAbout),        URLInfoAbout?.ToString());
				strWriter(nameof(URLUpdateInfo),       URLUpdateInfo?.ToString());
				strWriter(nameof(AuthorizedCDFPrefix), AuthorizedCDFPrefix);
				strWriter(nameof(Comments),            Comments);
				strWriter(nameof(Contact),             Contact);
				intWriter(nameof(Language),            Language);
				strWriter(nameof(ModifyPath),          ModifyPath);
				strWriter(nameof(Readme),              Readme);
				strWriter(nameof(UninstallString),     UninstallString);
				strWriter(nameof(SettingsIdentifier),  SettingsIdentifier);
				intWriter(nameof(NoRepair),            NoRepair.HasValue ? (uint?)Convert.ToInt32(NoRepair, CultureInfo.InvariantCulture) : null);
				intWriter(nameof(NoModify),            NoModify.HasValue ? (uint?)Convert.ToInt32(NoModify, CultureInfo.InvariantCulture) : null);
				intWriter(nameof(EstimatedSize),       EstimatedSize);
			}

			public void Nullcheck()
			{
				_ = DisplayIcon     ?? throw new NullReferenceException(nameof(DisplayIcon)     + " can not be null");
				_ = DisplayName     ?? throw new NullReferenceException(nameof(DisplayName)     + " can not be null");
				_ = Publisher       ?? throw new NullReferenceException(nameof(Publisher)       + " can not be null");
				_ = Version         ?? throw new NullReferenceException(nameof(Version)         + " can not be null");
				_ = VersionMajor    ?? throw new NullReferenceException(nameof(VersionMajor)    + " can not be null");
				_ = VersionMinor    ?? throw new NullReferenceException(nameof(VersionMinor)    + " can not be null");
				_ = UninstallString ?? throw new NullReferenceException(nameof(UninstallString) + " can not be null");
				_ = EstimatedSize   ?? throw new NullReferenceException(nameof(EstimatedSize)   + " can not be null");
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
		{ get => AppdataLocalDir + Path.DirectorySeparatorChar + ApplicationIdentifier; }

		public string InstallExePath
		{ get => InstallDir + Path.DirectorySeparatorChar + ApplicationIdentifier + ".exe"; }

		// TODO: add /Refresh as a property

		public string StartMinimizedCommand
		{ get => InstallExePath + " /Background"; }

		public string UninstallCommand
		{ get => InstallExePath + " /Uninstall"; }

		public string CloseCommand
		{ get => InstallExePath + " /Close"; }

		public string StartMenuLnkPath
		{ get => StartmenuProgramsDir + Path.DirectorySeparatorChar + ApplicationIdentifier + ".lnk"; }

		public string ScheduledTaskName
		{ get => ApplicationIdentifier + " - Check for updated config"; }

		// Registry Namespaces
		private static string rnsRun
		{ get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"; }
		private string rnsUninstall
		{ get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + ApplicationIdentifier; }


		// Public interface

		/// <summary>
		/// If false, it is probably running from %HOME%/Downloads or something
		/// </summary>
		public bool IsInstalled
		{ get => File.Exists(InstallExePath); }

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
				var d1 = File.GetLastWriteTime(ThisExePath);
				var d2 = File.GetLastWriteTime(InstallExePath);
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
				throw new EduroamConfigure.EduroamAppUserException("already installed", // TODO: use a more fitting exception?
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
			var binaryExe = File.ReadAllBytes(ThisExePath);
			File.WriteAllBytes(InstallExePath, binaryExe);

			// Register the application in Windows
			ApplicationMetadata.Write(
				intWriter: (string key, uint? value) =>
				{
					if (value == null) return; // ignore null values
					Debug.WriteLine("Write int to {0}\\{1}: {2}", rnsUninstall, key, value);
					Registry.SetValue(rnsUninstall, key, value, RegistryValueKind.DWord);
				},
				strWriter: (string key, string value) =>
				{
					if (value == null) return; // ignore null values
					Debug.WriteLine("Write str to {0}\\{1}: {2}", rnsUninstall, key, value);
					Registry.SetValue(rnsUninstall, key, value);
				});

			// Add shortcut to start menu
			Debug.WriteLine("Create shortcut: " + StartMenuLnkPath);
			if (!File.Exists(StartMenuLnkPath))
				File.Delete(StartMenuLnkPath);
			var wshell = new IWshRuntimeLibrary.WshShell();
			var lnk = wshell.CreateShortcut(StartMenuLnkPath) as IWshRuntimeLibrary.IWshShortcut;
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

				if (ApplicationMetadata.Publisher != null)
					task.RegistrationInfo.Author = ApplicationMetadata.Publisher;

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
				EduroamConfigure.ConnectToEduroam.RemoveAllWLANProfiles();
				EduroamConfigure.CertificateStore.UninstallAllInstalledCertificates(abortOnFail: true, omitRootCa: false);
			}
			catch (Exception)
			{
				shutdownAction(false);
				return;
			}
			EduroamConfigure.LetsWifi.WipeTokens();
			EduroamConfigure.PersistingStore.IdentityProvider = null;

			// Remove start menu link
			Debug.WriteLine("Delete file: " + StartMenuLnkPath);
			if (File.Exists(StartMenuLnkPath)) File.Delete(StartMenuLnkPath);

			// remove update task
			Debug.WriteLine("Delete scheduled task: " + ScheduledTaskName);
			using (var ts = new TaskService())
				ts.RootFolder.DeleteTask(ScheduledTaskName,
					exceptionOnNotExists: false);

			// remove registry entries
			Debug.WriteLine("Delete registry value: " + rnsRun + "\\" + ApplicationIdentifier);
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
				if (key?.GetValue(ApplicationIdentifier) != null)
					key.DeleteValue(ApplicationIdentifier);
			Debug.WriteLine("Delete registry subkey: " + rnsUninstall); ;
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", writable: true))
				if (key?.OpenSubKey(ApplicationIdentifier) != null)
					key.DeleteSubKeyTree(ApplicationIdentifier); // TODO: for some reason this doesn't seem to work

			// Delete myself:
			if (File.Exists(InstallExePath) && doDeleteSelf)
			{
				// this process delays 3 seconds then deletes the exe file
				var extinguishMe = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C choice /C Y /N /D Y /T 5 " +
						"& Del " + ShellEscape(InstallExePath) +
						"& Del /Q " + ShellEscape(InstallDir) +
						"& rmdir " + ShellEscape(InstallDir),
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
