using System.Globalization;

namespace EduRoam.Connect.Tasks
{
    public class Status
    {
        public string? ProfileName { get; set; }

        public DateTime? ExpirationDate { get; set; }

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
                        return string.Format(Resource.DaysLeft, diffDate.Value.Days.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (diffDate.Value.Hours > 1)
                    {
                        return string.Format(Resource.HoursLeft, diffDate.Value.Hours.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        return string.Format(Resource.MinutesLeft, diffDate.Value.Minutes.ToString(CultureInfo.InvariantCulture));
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
                    return Resource.VersionDebugNoVersion;
#else
                    return Resource.VersionReleaseNoVersion;
#endif
                }
                else
                {
#if DEBUG
                    return string.Format(Resource.VersionDebug, versionNumber);
#else
				    return string.Format(Resource.VersionRelease, versionNumber);
#endif
                }
            }
        }
    }
}
