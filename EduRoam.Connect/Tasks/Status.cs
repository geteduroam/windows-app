using EduRoam.Connect.Store;
using EduRoam.Localization;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EduRoam.Connect.Tasks
{
    public class Status
    {
        public IdentityProviderInfo? Identity { get; set; }

        public string? ProfileName => this.Identity?.DisplayName;

        public DateTime? ExpirationDate => this.Identity?.NotAfter;

        [MemberNotNullWhen(true, nameof(Identity))]
        [MemberNotNullWhen(true, nameof(ProfileName))]
        public bool ActiveProfile => this.Identity != null;

        public string TimeLeft
        {
            get
            {
                if (this.ExpirationDate == null)
                {
                    return "-";
                }
                else
                {
                    var diffDate = this.ExpirationDate - DateTime.Now;

                    if (diffDate.Value.Days > 1)
                    {
                        return string.Format(Resources.DaysLeft, diffDate.Value.Days.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (diffDate.Value.Hours > 1)
                    {
                        return string.Format(Resources.HoursLeft, diffDate.Value.Hours.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        return string.Format(Resources.MinutesLeft, diffDate.Value.Minutes.ToString(CultureInfo.InvariantCulture));
                    }
                }

            }
        }

        public string Version
        {
            get
            {
                var versionNumber = LetsWifi.Instance.VersionNumber;
                if (versionNumber == null)
                {
#if DEBUG
                    return Resources.VersionDebugNoVersion;
#else
                    return Resources.VersionReleaseNoVersion;
#endif
                }
                else
                {
#if DEBUG
                    return string.Format(Resources.VersionDebug, versionNumber);
#else
				    return string.Format(Resources.VersionRelease, versionNumber);
#endif
                }
            }
        }
    }
}
