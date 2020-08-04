using EduroamConfigure;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;


// TODO: test register of run on boot

namespace EduroamApp
{
	/// <summary>
	/// Because reinventing the wheel is fun.
	/// This is probably not achievable with the provided installer?
	/// </summary>
	class SelfInstaller
	{
		private readonly string ApplicationIdentifier;
		private ApplicationMeta ApplicationMetadata;

		public SelfInstaller(
			string applicationIdentifier,
			ApplicationMeta applicationMetadata)
		{
			_ = applicationIdentifier ?? throw new ArgumentNullException(paramName: nameof(applicationIdentifier), message: "name should not be null");

			ApplicationIdentifier = applicationIdentifier;

			applicationMetadata.SetRequired(this);
			applicationMetadata.Nullcheck();
			ApplicationMetadata = applicationMetadata;
		}

		public struct ApplicationMeta
		{
			// See https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
			private string DisplayIcon;         // [SET AUTOMATICALLY]
			public  string DisplayName;         // [REQUIRED] ProductName
			private string DisplayVersion       // [SET AUTOMATICALLY] Derived from ProductVersion
			{ get => Version; }
			public  string Publisher;           // [REQUIRED] Manufacturer
			private string Version              // [SET AUTOMATICALLY] Derived from ProductVersion
			{ get => Application.ProductVersion; } // TODO: make sure this is correct, and derive MajorVersion and MinorVersion
			public  string HelpLink;            // ARPHELPLINK
			public  string HelpTelephone;       // ARPHELPTELEPHONE
			private string InstallDate          // [SET AUTOMATICALLY] The last time this product received service. The value of this property is replaced each time a patch is applied or removed from the product or the /v Command-Line Option is used to repair the product. If the product has received no repairs or patches this property contains the time this product was installed on this computer.
			{ get => DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture); }
			private string InstallLocation;     // [SET AUTOMATICALLY] ARPINSTALLLOCATION
			public  string InstallSource;       // SourceDir
			public  string URLInfoAbout;        // ARPURLINFOABOUT
			public  string URLUpdateInfo;       // ARPURLUPDATEINFO
			public  string AuthorizedCDFPrefix; // ARPAUTHORIZEDCDFPREFIX
			public  string Comments;            // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
			public  string Contact;             // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
			public  uint?  Language;            // ProductLanguage
			private string ModifyPath;          // [SET AUTOMATICALLY] "Determined and set by the Windows Installer."
			public  string Readme;              // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
			private string UninstallString;     // [SET AUTOMATICALLY] "Determined and set by Windows Installer."
			public  string SettingsIdentifier;  // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
			public  bool?  NoRepair;            // REG_DWORD
			public  bool?  NoModify;            // REG_DWORD
			private uint?  EstimatedSize;       // [SET AUTOMATICALLY] REG_DWORD


			// for SelfInstaller to use:

			public void SetRequired(
				SelfInstaller installer)
			{
				DisplayIcon     = installer.InstallExePath;
				InstallLocation = installer.InstallDir;
				UninstallString = installer.UninstallCommand;
				EstimatedSize   = (uint)new FileInfo(SelfInstaller.ThisExePath).Length;
				ModifyPath      = null;
				// TODO: SettingsIdentifier ?
			}

			public void Write(
				Action<string, string> strWriter,
				Action<string, uint?> intWriter)
			{
				strWriter(nameof(DisplayIcon),         DisplayIcon);
				strWriter(nameof(DisplayName),         DisplayName);
				strWriter(nameof(DisplayVersion),      DisplayVersion);
				strWriter(nameof(Publisher),           Publisher);
				//strWriter(nameof(VersionMinor),        VersionMinor);
				//strWriter(nameof(VersionMajor),        VersionMajor);
				strWriter(nameof(Version),             Version);
				strWriter(nameof(HelpLink),            HelpLink);
				strWriter(nameof(HelpTelephone),       HelpTelephone);
				strWriter(nameof(InstallDate),         InstallDate);
				strWriter(nameof(InstallLocation),     InstallLocation);
				strWriter(nameof(InstallSource),       InstallSource);
				strWriter(nameof(URLInfoAbout),        URLInfoAbout);
				strWriter(nameof(URLUpdateInfo),       URLUpdateInfo);
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
				_ = DisplayIcon      ?? throw new NullReferenceException(nameof(DisplayIcon) +     " should not be null");
				_ = DisplayName      ?? throw new NullReferenceException(nameof(DisplayName) +     " should not be null");
				_ = DisplayVersion   ?? throw new NullReferenceException(nameof(DisplayVersion) +  " should not be null");
				_ = Publisher        ?? throw new NullReferenceException(nameof(Publisher) +       " should not be null");
				_ = Version          ?? throw new NullReferenceException(nameof(Version) +         " should not be null");
				_ = UninstallString  ?? throw new NullReferenceException(nameof(UninstallString) + " should not be null");
				_ = EstimatedSize    ?? throw new NullReferenceException(nameof(EstimatedSize) +   " should not be null");
			}
		}


		// Shorhands

		public static string AppdataLocalDir
		{ get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
		public static string StartmenuProgramsDir
		{ get => Environment.GetFolderPath(Environment.SpecialFolder.Programs); }
		public static string ThisExePath
		{ get => Application.ExecutablePath; }
		public string InstallDir
		{ get => AppdataLocalDir + Path.DirectorySeparatorChar + ApplicationIdentifier; }
		public string InstallExePath
		{ get => InstallDir + Path.DirectorySeparatorChar + ApplicationIdentifier + ".exe"; }
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
		private string rnsMeta
		{ get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + ApplicationIdentifier; }


		// Public interface

		/// <summary>
		/// If false, it is probably running from %HOME%/Downloads or something
		/// </summary>
		public bool IsRunningFromInstallDir
		{ get => InstallExePath == ThisExePath; }

		/// <summary>
		/// Installs the running EXE to %USER%/AppData/Local,
		/// registering it to the registry for the current user
		/// </summary>
		public void InstallToUserLocal()
		{
			// avoid uneccesary/illegal updates
			if (IsRunningFromInstallDir) // sanity check, should never happen
				throw new EduroamAppUserError("already installed", // TODO: use a more fitting exception?
					"This application has already been installed. Installing it again won't have any effect.");
			if (File.Exists(InstallExePath))
			{
				var d1 = File.GetLastWriteTime(ThisExePath);
				var d2 = File.GetLastWriteTime(InstallExePath);
				if (d1 <= d2)
				{
					Console.WriteLine(
						"The date of the currently installed version is equal to or newer than this one.");
					return;
				}
			}

			// Create target install directory
			if (!Directory.Exists(InstallDir))
				Directory.CreateDirectory(InstallDir);

			// close running instance, to allow for overwriting it.
			// TODO, then relaunch it when complete

			// write executable
			File.Copy(ThisExePath, InstallExePath, overwrite: true);

			// Register the application in Windows
			ApplicationMetadata.Write(
				intWriter: (string key, uint? value) =>
				{
					if (value == null) return;
					Debug.WriteLine("Write int to {0}\\{1}: {2}", rnsMeta, key, value);
					Registry.SetValue(rnsMeta, key, value, RegistryValueKind.DWord);
				},
				strWriter: (string key, string value) =>
				{
					if (value == null) return;
					Debug.WriteLine("Write str to {0}\\{1}: {2}", rnsMeta, key, value);
					Registry.SetValue(rnsMeta, key, value);
				});

			// Add shortcut to start menu
			Debug.WriteLine("Write file " + StartMenuLnkPath);
			if (!File.Exists(StartMenuLnkPath))
				File.Delete(StartMenuLnkPath);
			var wshell = new IWshRuntimeLibrary.WshShell();
			var lnk = wshell.CreateShortcut(StartMenuLnkPath) as IWshRuntimeLibrary.IWshShortcut;
			lnk.TargetPath = InstallExePath;
			lnk.WorkingDirectory = InstallDir;
			lnk.Save();

			// Register the application to run on boot
			Debug.WriteLine("Write str to {0}\\{1}: {2}", rnsRun, ApplicationIdentifier, StartMinimizedCommand);
			Registry.SetValue(rnsRun, ApplicationIdentifier, StartMinimizedCommand);

			// Register scheduled task to check for updates
			Debug.WriteLine("Create scheduled task: " + ScheduledTaskName);
			using (var ts = new TaskService())
			{
				var task = ts.NewTask();
				task.Settings.AllowDemandStart = true;
				task.Settings.StartWhenAvailable = true; // run as soon as possible after a schedule start is missed
				task.Settings.DisallowStartIfOnBatteries = false;

				if (ApplicationMetadata.Publisher != null)
					task.RegistrationInfo.Author = ApplicationMetadata.Publisher;

				task.Actions.Add(new ExecAction(InstallExePath, arguments: "/Refresh"));

				task.Triggers.Add(new DailyTrigger(daysInterval: 3) { // every 3 days
					StartBoundary = DateTime.Today.AddHours(12) }); // around noon

				ts.RootFolder.RegisterTaskDefinition(ScheduledTaskName, task);
			}

		}

		/// <summary>
		/// Uninstalls the program.
		/// Will cause the program to exit.
		/// </summary>
		public void ExitAndUninstallSelf(bool usingWinforms = true)
		{
			ConnectToEduroam.RemoveAllProfiles();

			CertificateStore.UninstallAllInstalledCertificates();

			LetsWifi.WipeTokens();

			// Remove start menu link
			Debug.WriteLine("Delete file: " + StartMenuLnkPath);
			if (!File.Exists(StartMenuLnkPath)) File.Delete(StartMenuLnkPath);

			// remove registry entries
			Debug.WriteLine("Delete registry subkey: " + rnsMeta); ;
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", writable: true))
				if (key?.OpenSubKey(ApplicationIdentifier) != null)
					key.DeleteSubKey(ApplicationIdentifier);
			Debug.WriteLine("Delete registry value: " + rnsRun + "\\" + ApplicationIdentifier);
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
				if (key?.GetValue(ApplicationIdentifier) != null)
					key.DeleteValue(ApplicationIdentifier);

			// remove update task
			Debug.WriteLine("Delete scheduled task: " + ScheduledTaskName);
			using (var ts = new TaskService())
				ts.RootFolder.DeleteTask(ScheduledTaskName,
					exceptionOnNotExists: false);

			// Delete myself:
			if (File.Exists(InstallExePath))
			{
				// this process delays 3 seconds then deletes the exe file
				var killme = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C choice /C Y /N /D Y /T 3 " + // TODO: escape arguments
						"& Del " + InstallExePath +
						"& Del /Q " + InstallDir +
						"& rmdir " + InstallDir,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					WorkingDirectory = "C:\\"
				};
				Process.Start(killme);
			}

			// Quit
			//https://stackoverflow.com/a/12978034
			if (usingWinforms)
			{
				Application.Exit();
			}
			else
			{
				Environment.Exit(0);
			}
		}
	}
}
