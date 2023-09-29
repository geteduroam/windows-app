using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using WixSharp;

namespace App.MsiCreator.Commands
{
    internal class CreateMsi : ICommand
    {
        public static readonly string CommandName = "create";

        public static readonly string CommandDescription = "Create Msi (.msi) package";

        public Command GetCommand()
        {
            var installerTemplateOption = Options.GetInstallerTemplateOption();
            var exePathOption = Options.GetExePath();

            var command = new Command(CommandName, CommandDescription)
            {
                installerTemplateOption,
                exePathOption
            };

            command.SetHandler((FileInfo installerTemplatePath, FileInfo exePath) =>
            {
                var installerTemplateStr = System.IO.File.ReadAllText(installerTemplatePath.FullName);
                var installerTemplate = Newtonsoft.Json.JsonConvert.DeserializeObject<MsiTemplate>(installerTemplateStr);
                Create(installerTemplate, exePath);

                Console.WriteLine(".msi created");
                Console.ReadLine();
            }, installerTemplateOption, exePathOption);

            return command;
        }

        internal static void Create(MsiTemplate appTemplate, FileInfo exePath)
        {
            var project = new Project(appTemplate.AppTitle,
                          new Dir($"%ProgramFiles%\\{appTemplate.ProgramFolder}",
                              new WixSharp.File(exePath.FullName)))
            {
                GUID = appTemplate.InstallerId,
                UI = WUI.WixUI_ProgressOnly,
                Version = new Version(AssemblyName.GetAssemblyName(exePath.FullName).Version.ToString())
            };

            var appIconFileInfo = new FileInfo(appTemplate.AppIconPath);

            if (appIconFileInfo.Directory.FullName != Directory.GetCurrentDirectory())
            {
                appIconFileInfo = appIconFileInfo.CopyTo(Path.Combine(Directory.GetCurrentDirectory(), appIconFileInfo.Name), true);
            }
            project.ControlPanelInfo.ProductIcon = appIconFileInfo.Name;
            project.ControlPanelInfo.Manufacturer = appTemplate.Manufacturer;
            project.ControlPanelInfo.NoModify = true;

            // When not only showing progress (WixUI_ProgressOnly) but showing a minimal UI, set the following attributes
            // project.BackgroundImage = <Image path>
            // project.BannerImage  = <Image path>
            // project.LicenceFile = <path to .rtf file>;            

            var msi = Compiler.BuildMsi(project);

            if (msi == null)
            {
                Debug.WriteLine("Could not create .msi");
            }
            else
            {
                Debug.WriteLine($".msi created ({msi})");
            }
        }
    }
}
