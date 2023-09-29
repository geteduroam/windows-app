using System;

namespace App.MsiCreator.Commands
{
    internal class MsiTemplate
    {
        public string AppTitle { get; set; }

        public string ProgramFolder { get; set; }

        public Guid InstallerId { get; set; }

        public string? AppIconPath { get; set; }

        public string? Manufacturer { get; set; }

    }
}
