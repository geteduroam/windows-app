using Semver;

using System.Net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using App.Library.Models;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using App.Library.Install;

namespace App.Library.Utility;

public static class UpdateChecker
{
    private const string UpdateUrlBase = "{0}/windows/{1}/update.json"; // {0} has to be replaced with the base url from the settings and {1} has to be replaced with the arch
    private const string RegistryBase = @"Software\{0}"; // {0} has to be replaced with the applicationIdentifier

    public static UpdateResponseRootDto UpdateData { get; set; } = new();       
    public static bool IsUpdateAvailable { get; set; }
    public static SemVersion? MinimalSupportedVersion { get; set; } 
    public static SemVersion? NewVersion { get; set; }

    // http objects
    public async static Task<bool> CheckIfUpdateAvailableAsync()
    {
        if(!IsUpdateAllowedByPolicy(Registry.CurrentUser) || !IsUpdateAllowedByPolicy(Registry.LocalMachine))
        {
            return false;
        }

        await DownloadUpdateJsonAsync();
        try
        {
            NewVersion = SemVersion.Parse(UpdateData.CurrentVersion, SemVersionStyles.Strict);
        }
        catch (Exception) {
            Debug.WriteLine("Cannot parse version number from update data, continuing as if no update available; may happen if internet is down");
            return false;
        }

        var parsedMinimalSupportedVersion = Version.Parse(UpdateData.MinimalSupportedVersion);
        MinimalSupportedVersion = new SemVersion(parsedMinimalSupportedVersion.Major, parsedMinimalSupportedVersion.Minor, parsedMinimalSupportedVersion.Build);

        // If the app was already installed, we should already have aborted here 
        IsUpdateAvailable = SelfInstaller.DefaultInstance.CanUpdateRunning(NewVersion);

        return IsUpdateAvailable;
    }

    private static bool IsUpdateAllowedByPolicy(RegistryKey registryBaseKey)
    {
        var key = registryBaseKey.OpenSubKey(string.Format(RegistryBase, Settings.Settings.ApplicationName));
        return key == null || !Convert.ToBoolean(key.GetValue("DisableAutoUpdate", false));
    }

    /// <summary>
    /// Downloads the correct executable from the location specified in the updateUrl response
    /// </summary>
    /// <returns></returns>
    public static async Task DownloadUpdateAsync()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}_{Settings.Settings.ApplicationName}.exe");

            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(UpdateData.DownloadUrl, tempPath);
                try
                {
                    SelfInstaller.DefaultInstance.UpdateWithFile(tempPath, true);
                    SelfInstaller.DefaultInstance.StartApplicationFromInstallLocation();
                }
                finally
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (IOException _) { }
                }
                Environment.Exit(0);
            }

        } catch(Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    #region Private helper functions
    private static Uri GetUpdateUrl()
    {
        var arch = ArchitectureHelper.GetNativeArch();
        var updateUrl = string.Format(UpdateUrlBase, Settings.Settings.UpdateBaseUrl, arch.ToString().ToLower());

        return new Uri(updateUrl);
    }

    private static async Task DownloadUpdateJsonAsync()
    {
        var url = GetUpdateUrl();

        try
        {
            var webClient = new WebClient();
            var response = await webClient.DownloadStringTaskAsync(url);

            var deserializedObject = JsonConvert.DeserializeObject<UpdateResponseDto>(response);
            UpdateData = deserializedObject.UpdateRoot;
        } catch(Exception e)
        {
            // maybe log this?!
        }
    }
    #endregion

}
