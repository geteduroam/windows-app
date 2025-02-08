using System.Reflection;

namespace App.Settings
{
    public static class Settings
    {
        public static string OAuthClientId { get; set; } = "app.geteduroam.win";
        public static string ApplicationIdentifier { get; set; } = "geteduroam";
        public static string ApplicationTitle { get; set; } = "eduroam";
        public static string UpdateBaseUrl { get; set; } = "https://dl.eduroam.app";
        public static int DaysLeftForNotification { get; set; } = 10;
        public static string? EapConfigFileLocation { get; set; } = null;
        public static string HelpUrl { get; set; } = "https://geteduroam.app/";

        public static string AppTitle
        {
            get
            {
                var appAssemblyName = Assembly.GetEntryAssembly()!.GetName();

                if (!string.IsNullOrWhiteSpace(appAssemblyName.CultureName))
                {
                    return appAssemblyName.CultureName;
                }
                return appAssemblyName.Name ?? "geteduroam";
            }
        }
    }
}
