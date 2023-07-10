using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace App.Library
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
			this.ApplicationIdentifier = applicationIdentifier
                                         ?? throw new ArgumentNullException(paramName: nameof(applicationIdentifier));

			applicationMetadata.SetRequired(this);
			applicationMetadata.Nullcheck();
			this.ApplicationMetadata = applicationMetadata;
		}

		public struct ApplicationMeta
		{
			// See https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
			private string DisplayIcon;                       // [SET AUTOMATICALLY]
			public  string DisplayName          { get; set; } // ProductName
			private string DisplayVersion       { get => this.Version; } // [SET AUTOMATICALLY] Derived from ProductVersion
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
				this.DisplayIcon     = installer.InstallExePath;
				this.InstallLocation = installer.InstallDir;
				this.InstallDate     = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				this.UninstallString = installer.UninstallCommand;
				this.EstimatedSize   = (uint)new FileInfo(SelfInstaller.ThisExePath).Length / 1024;
				this.ModifyPath      = null;
				// TODO: SettingsIdentifier = ?
			}

			public void Write(
				Action<string, string> strWriter,
				Action<string, uint?> intWriter)
			{
				_ = strWriter ?? throw new ArgumentNullException(paramName: nameof(strWriter));
				_ = intWriter ?? throw new ArgumentNullException(paramName: nameof(intWriter));
				strWriter(nameof(this.DisplayIcon),         this.DisplayIcon);
				strWriter(nameof(this.DisplayName),         this.DisplayName);
				strWriter(nameof(this.DisplayVersion),      this.DisplayVersion);
				strWriter(nameof(this.Publisher),           this.Publisher);
				strWriter(nameof(this.Version),             this.Version);
				strWriter(nameof(this.VersionMajor),        this.VersionMajor);
				strWriter(nameof(this.VersionMinor),        this.VersionMinor);
				strWriter(nameof(this.HelpLink),            this.HelpLink);
				strWriter(nameof(this.HelpTelephone),       this.HelpTelephone);
				strWriter(nameof(this.InstallDate),         this.InstallDate);
				strWriter(nameof(this.InstallLocation),     this.InstallLocation);
				strWriter(nameof(this.InstallSource),       this.InstallSource);
				strWriter(nameof(this.URLInfoAbout),        this.URLInfoAbout?.ToString());
				strWriter(nameof(this.URLUpdateInfo),       this.URLUpdateInfo?.ToString());
				strWriter(nameof(this.AuthorizedCDFPrefix), this.AuthorizedCDFPrefix);
				strWriter(nameof(this.Comments),            this.Comments);
				strWriter(nameof(this.Contact),             this.Contact);
				intWriter(nameof(this.Language),            this.Language);
				strWriter(nameof(this.ModifyPath),          this.ModifyPath);
				strWriter(nameof(this.Readme),              this.Readme);
				strWriter(nameof(this.UninstallString),     this.UninstallString);
				strWriter(nameof(this.SettingsIdentifier),  this.SettingsIdentifier);
				intWriter(nameof(this.NoRepair),            this.NoRepair.HasValue ? (uint?)Convert.ToInt32(this.NoRepair, CultureInfo.InvariantCulture) : null);
				intWriter(nameof(this.NoModify),            this.NoModify.HasValue ? (uint?)Convert.ToInt32(this.NoModify, CultureInfo.InvariantCulture) : null);
				intWriter(nameof(this.EstimatedSize),       this.EstimatedSize);
			}

			public void Nullcheck()
			{
				_ = this.DisplayIcon     ?? throw new NullReferenceException(nameof(this.DisplayIcon)     + " can not be null");
				_ = this.DisplayName     ?? throw new NullReferenceException(nameof(this.DisplayName)     + " can not be null");
				_ = this.Publisher       ?? throw new NullReferenceException(nameof(this.Publisher)       + " can not be null");
				_ = this.Version         ?? throw new NullReferenceException(nameof(this.Version)         + " can not be null");
				_ = this.VersionMajor    ?? throw new NullReferenceException(nameof(this.VersionMajor)    + " can not be null");
				_ = this.VersionMinor    ?? throw new NullReferenceException(nameof(this.VersionMinor)    + " can not be null");
				_ = this.UninstallString ?? throw new NullReferenceException(nameof(this.UninstallString) + " can not be null");
				_ = this.EstimatedSize   ?? throw new NullReferenceException(nameof(this.EstimatedSize)   + " can not be null");
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
		{ get => AppdataLocalDir + Path.DirectorySeparatorChar + this.ApplicationIdentifier; }

		public string InstallExePath
		{ get => this.InstallDir + Path.DirectorySeparatorChar + this.ApplicationIdentifier + ".exe"; }

		// TODO: add /Refresh as a property

		public string StartMinimizedCommand
		{ get => this.InstallExePath + " /Background"; }

		public string UninstallCommand
		{ get => this.InstallExePath + " /Uninstall"; }

		public string CloseCommand
		{ get => this.InstallExePath + " /Close"; }

		public string StartMenuLnkPath
		{ get => StartmenuProgramsDir + Path.DirectorySeparatorChar + this.ApplicationIdentifier + ".lnk"; }

		public string ScheduledTaskName
		{ get => this.ApplicationIdentifier + " - Check for updated config"; }

		// Registry Namespaces
		private static string rnsRun
		{ get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"; }
		private string rnsUninstall
		{ get => "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + this.ApplicationIdentifier; }


		// Public interface

		/// <summary>
		/// If false, it is probably running from %HOME%/Downloads or something
		/// </summary>
		public bool IsInstalled
		{ get => File.Exists(this.InstallExePath); }

		/// <summary>
		/// If false, it is probably running from %HOME%/Downloads or something
		/// </summary>
		public bool IsRunningInInstallLocation
		{ get => this.InstallExePath == ThisExePath; }

		public void EnsureIsInstalled()
		{
			if (this.IsRunningInInstallLocation) return; // TODO: some flow to update itself
			if (this.IsInstalled)
			{
				// TODO: assemblyversion instad of file date
				var d1 = File.GetLastWriteTime(ThisExePath);
				var d2 = File.GetLastWriteTime(this.InstallExePath);
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

			this.InstallToUserLocal();
		}

		/// <summary>
		/// Installs the running EXE to %USER%/AppData/Local,
		/// registering it to the registry for the current user
		/// </summary>
		private void InstallToUserLocal()
		{
			// avoid uneccesary/illegal updates
			//todo new //if (this.IsRunningInInstallLocation) // sanity check, should never happen
			//	throw new EduroamConfigure.EduroamAppUserException("already installed", // TODO: use a more fitting exception?
			//		"This application has already been installed. " +
			//		"Installing it again won't have any effect.");

			// Create target install directory
			if (!Directory.Exists(this.InstallDir))
				Directory.CreateDirectory(this.InstallDir);

			// write executable, not retaining Zone.Identifier NTFS stream
			/*
			File.Copy(ThisExePath, InstallExePath, overwrite: true); // BAD: keeps NTFS streams which we don't want
			*/
			//
			// Reading and writing manually works better, because then the resulting .exe can be openend
			// at startup or by the scheduler without the user getting "Are you sure you want to run this software?"
			var binaryExe = File.ReadAllBytes(ThisExePath);
			File.WriteAllBytes(this.InstallExePath, binaryExe);

			// Register the application in Windows
			this.ApplicationMetadata.Write(
				intWriter: (string key, uint? value) =>
				{
					if (value == null) return; // ignore null values
					Debug.WriteLine("Write int to {0}\\{1}: {2}", this.rnsUninstall, key, value);
					Registry.SetValue(this.rnsUninstall, key, value, RegistryValueKind.DWord);
				},
				strWriter: (string key, string value) =>
				{
					if (value == null) return; // ignore null values
					Debug.WriteLine("Write str to {0}\\{1}: {2}", this.rnsUninstall, key, value);
					Registry.SetValue(this.rnsUninstall, key, value);
				});

			// Add shortcut to start menu
			Debug.WriteLine("Create shortcut: " + this.StartMenuLnkPath);
			if (!File.Exists(this.StartMenuLnkPath))
				File.Delete(this.StartMenuLnkPath);
			var wshell = new IWshRuntimeLibrary.WshShell();
			var lnk = wshell.CreateShortcut(this.StartMenuLnkPath) as IWshRuntimeLibrary.IWshShortcut;
			lnk.TargetPath = this.InstallExePath;
			lnk.WorkingDirectory = this.InstallDir;
			lnk.Save();

			// Register scheduled task to check for updates
			Debug.WriteLine("Create scheduled task: " + this.ScheduledTaskName);
			using (var ts = new TaskService())
			{
				var task = ts.NewTask();
				task.Settings.AllowDemandStart = true;
				task.Settings.StartWhenAvailable = true; // run as soon as possible after a scheduled start is missed
				task.Settings.DisallowStartIfOnBatteries = false;

				if (this.ApplicationMetadata.Publisher != null)
					task.RegistrationInfo.Author = this.ApplicationMetadata.Publisher;

				task.Actions.Add(new ExecAction(this.InstallExePath, arguments: "/Refresh"));

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
					ts.RootFolder.RegisterTaskDefinition(this.ScheduledTaskName, task);
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
				//todo new EduroamConfigure.ConnectToEduroam.RemoveAllWLANProfiles();
				//todo new EduroamConfigure.CertificateStore.UninstallAllInstalledCertificates(abortOnFail: true, omitRootCa: false);
			}
			catch (Exception)
			{
				shutdownAction(false);
				return;
			}
            //todo new EduroamConfigure.LetsWifi.WipeTokens();
            //todo new EduroamConfigure.PersistingStore.IdentityProvider = null;

            // Remove start menu link
            Debug.WriteLine("Delete file: " + this.StartMenuLnkPath);
			if (File.Exists(this.StartMenuLnkPath)) File.Delete(this.StartMenuLnkPath);

			// remove update task
			Debug.WriteLine("Delete scheduled task: " + this.ScheduledTaskName);
			using (var ts = new TaskService())
				ts.RootFolder.DeleteTask(this.ScheduledTaskName,
					exceptionOnNotExists: false);

			// remove registry entries
			Debug.WriteLine("Delete registry value: " + rnsRun + "\\" + this.ApplicationIdentifier);
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true))
				if (key?.GetValue(this.ApplicationIdentifier) != null)
					key.DeleteValue(this.ApplicationIdentifier);
			Debug.WriteLine("Delete registry subkey: " + this.rnsUninstall); ;
			using (RegistryKey key = Registry.CurrentUser
					.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", writable: true))
				if (key?.OpenSubKey(this.ApplicationIdentifier) != null)
					key.DeleteSubKeyTree(this.ApplicationIdentifier); // TODO: for some reason this doesn't seem to work

			// Delete myself:
			if (File.Exists(this.InstallExePath) && doDeleteSelf)
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
