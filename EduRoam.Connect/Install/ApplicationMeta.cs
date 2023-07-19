using System.Globalization;

namespace EduRoam.Connect.Install
{
    public partial class SelfInstaller
    {
        public struct ApplicationMeta
        {
            // See https://docs.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
            private string DisplayIcon { get; set; }                       // [SET AUTOMATICALLY]
            public string DisplayName { get; set; } // ProductName
            private string DisplayVersion { get => Version; } // [SET AUTOMATICALLY] Derived from ProductVersion
            public string Publisher { get; set; } // Manufacturer
            public string Version { get; set; } // Derived from ProductVersion
            public string VersionMajor { get; set; } // Derived from ProductVersion
            public string VersionMinor { get; set; } // Derived from ProductVersion
            public string HelpLink { get; set; } // ARPHELPLINK
            public string HelpTelephone { get; set; } // ARPHELPTELEPHONE
            private string InstallDate { get; set; }                     // [SET AUTOMATICALLY] The last time this product received service.
            private string InstallLocation { get; set; }                  // [SET AUTOMATICALLY] ARPINSTALLLOCATION
            public string InstallSource { get; set; } // SourceDir
            public Uri URLInfoAbout { get; set; } // ARPURLINFOABOUT
            public Uri URLUpdateInfo { get; set; } // ARPURLUPDATEINFO
            public string AuthorizedCDFPrefix { get; set; } // ARPAUTHORIZEDCDFPREFIX
            public string Comments { get; set; } // [NICE TO HAVE] ARPCOMMENTS. Comments provided to the Add or Remove Programs control panel.
            public string Contact { get; set; } // [NICE TO HAVE] ARPCONTACT. Contact provided to the Add or Remove Programs control panel.
            public uint? Language { get; set; } // ProductLanguage
            private string? ModifyPath { get; set; }                         // [SET AUTOMATICALLY] "Determined and set by the Windows Installer."
            public string Readme { get; set; } // [NICE TO HAVE] ARPREADME. Readme provided to the Add or Remove Programs control panel.
            private string UninstallString { get; set; }                   // [SET AUTOMATICALLY] "Determined and set by Windows Installer."
            public string SettingsIdentifier { get; set; } // MSIARPSETTINGSIDENTIFIER. contains a semi-colon delimited list of the registry locations where the application stores a user's settings and preferences.
            public bool? NoRepair { get; set; } // REG_DWORD
            public bool? NoModify { get; set; } // REG_DWORD
            private uint? EstimatedSize { get; set; }                    // [SET AUTOMATICALLY] REG_DWORD


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
                strWriter(nameof(this.DisplayIcon), this.DisplayIcon);
                strWriter(nameof(this.DisplayName), this.DisplayName);
                strWriter(nameof(this.DisplayVersion), this.DisplayVersion);
                strWriter(nameof(this.Publisher), this.Publisher);
                strWriter(nameof(this.Version), this.Version);
                strWriter(nameof(this.VersionMajor), this.VersionMajor);
                strWriter(nameof(this.VersionMinor), this.VersionMinor);
                strWriter(nameof(this.HelpLink), this.HelpLink);
                strWriter(nameof(this.HelpTelephone), this.HelpTelephone);
                strWriter(nameof(this.InstallDate), this.InstallDate);
                strWriter(nameof(this.InstallLocation), this.InstallLocation);
                strWriter(nameof(this.InstallSource), this.InstallSource);
                strWriter(nameof(this.URLInfoAbout), this.URLInfoAbout?.ToString() ?? string.Empty);
                strWriter(nameof(this.URLUpdateInfo), this.URLUpdateInfo?.ToString() ?? string.Empty);
                strWriter(nameof(this.AuthorizedCDFPrefix), this.AuthorizedCDFPrefix);
                strWriter(nameof(this.Comments), this.Comments);
                strWriter(nameof(this.Contact), this.Contact);
                intWriter(nameof(this.Language), this.Language);
                strWriter(nameof(this.ModifyPath), this.ModifyPath ?? string.Empty);
                strWriter(nameof(this.Readme), this.Readme);
                strWriter(nameof(this.UninstallString), this.UninstallString);
                strWriter(nameof(this.SettingsIdentifier), this.SettingsIdentifier);
                intWriter(nameof(this.NoRepair), this.NoRepair.HasValue ? (uint?)Convert.ToInt32(this.NoRepair, CultureInfo.InvariantCulture) : null);
                intWriter(nameof(this.NoModify), this.NoModify.HasValue ? (uint?)Convert.ToInt32(this.NoModify, CultureInfo.InvariantCulture) : null);
                intWriter(nameof(this.EstimatedSize), this.EstimatedSize);
            }

            public readonly void Nullcheck()
            {
                _ = this.DisplayIcon ?? throw new NullReferenceException(nameof(this.DisplayIcon) + " can not be null");
                _ = this.DisplayName ?? throw new NullReferenceException(nameof(this.DisplayName) + " can not be null");
                _ = this.Publisher ?? throw new NullReferenceException(nameof(this.Publisher) + " can not be null");
                _ = this.Version ?? throw new NullReferenceException(nameof(this.Version) + " can not be null");
                _ = this.VersionMajor ?? throw new NullReferenceException(nameof(this.VersionMajor) + " can not be null");
                _ = this.VersionMinor ?? throw new NullReferenceException(nameof(this.VersionMinor) + " can not be null");
                _ = this.UninstallString ?? throw new NullReferenceException(nameof(this.UninstallString) + " can not be null");
                _ = this.EstimatedSize ?? throw new NullReferenceException(nameof(this.EstimatedSize) + " can not be null");
            }
        }

    }
}
