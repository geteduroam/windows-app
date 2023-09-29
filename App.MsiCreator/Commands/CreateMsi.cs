using System;
using System.CommandLine;
using System.IO;

using WixSharp;

namespace App.MsiCreator.Commands
{
    internal class CreateMsi : ICommand
    {
        public static readonly string CommandName = "create";

        public static readonly string CommandDescription = "Create Msi (.msi) package";

        public Command GetCommand()
        {
            var appOption = Options.GetAppOption();
            var exePathOption = Options.GetExePath();

            var command = new Command(CommandName, CommandDescription)
            {
                appOption,
                exePathOption
            };

            command.SetHandler((string app, FileInfo exePath) =>
            {
                Create(app, exePath);

                Console.WriteLine(".msi created");
                Console.ReadLine();
            }, appOption, exePathOption);

            return command;
        }

        internal static void Create(string app, FileInfo exePath)
        {
            var project = new Project(app,
                          new Dir($"%ProgramFiles%\\{app}",
                              new WixSharp.File(exePath.FullName)));

            project.GUID = Guid.NewGuid();

            var msi = Compiler.BuildMsi(project);

            if (msi == null)
            {
                Console.WriteLine("Could not create .msi");
            }
            else
            {
                Console.WriteLine($".msi created ({msi})");
            }
        }
    }
}
