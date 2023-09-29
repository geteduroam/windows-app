using System.CommandLine;
using System.IO;

namespace App.MsiCreator.Commands
{
    public static class Options
    {
        public static Option<FileInfo> GetInstallerTemplateOption()
        {
            return new Option<FileInfo>(
                aliases: new string[] { "-t", "--template" },
                description: "Path to installer json. For example: c:\\test\\geteduroam-installer.json");
        }

        public static Option<FileInfo> GetExePath() => new Option<FileInfo>(
                aliases: new string[] { "-e", "--exe" },
                description: "Path to executable. For example: c:\\test\\geteduroam.exe");

    }
}
