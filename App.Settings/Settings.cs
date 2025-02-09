namespace App.Settings
{
    public static class Settings
    {
        public static string OAuthClientId { get; set; } = "app.geteduroam.win";
        public static string ApplicationName { get; set; } = "geteduroam";
        public static string NetworkName { get; set; } = "eduroam";
        public static string Publisher { get; set; } = "SURF";
        public static string UpdateBaseUrl { get; set; } = "https://dl.eduroam.app";
        public static int DaysLeftForNotification { get; set; } = 10;
        public static string? EapConfigFileLocation { get; set; } = null;
        public static string HelpUrl { get; set; } = "https://geteduroam.app/";
    }
}
