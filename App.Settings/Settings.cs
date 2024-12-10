using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

using Semver;

namespace App.Settings
{
    public static class Settings
    {
        public static string OAuthClientId { get; set; } = "app.geteduroam.win";
        public static string ApplicationIdentifier { get; set; } = "geteduroam";
        public static string UpdateBaseUrl { get; set; } = "https://dl.eduroam.app";
        public static int DaysLeftForNotification { get; set; } = 10;
        public static string? EapConfigFileLocation { get; set; } = null;
        public static SemVersion ApplicationVersion { get; } = GetApplicationVersion();

        private static SemVersion GetApplicationVersion()
        {
            return GetFileVersion(Assembly.GetEntryAssembly().Location);
        }

        public static SemVersion GetFileVersion(string path)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(path);
            var v = fileVersion.FileVersion;
            var splittedVersion = v.Split(".".ToCharArray());


            return new SemVersion(int.Parse(splittedVersion[0]), int.Parse(splittedVersion[1]), int.Parse(splittedVersion[2]));
        }
    }
}
