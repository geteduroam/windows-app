# App.MsiCreator

`App.MsiCreator` is a .NET 8 CLI utility that builds enterprise Windows Installer (`.msi`) packages for geteduroam, getgovroam, and eduroam-cli using **WiX v4** and **WixSharp** (`WixSharp_wix4.bin` 2.14.1).

- **Target Framework**: `net8.0-windows10.0.19041.0` (Console Application)
- **Engine**: WiX v4 (integrated via NuGet, requiring no external tool installation on build hosts)

---

## Features

- **Automated PE Architecture Detection**: Inspects the target binary PE header (`0xAA64` for ARM64, `0x8664` for AMD64) and automatically sets WiX compiler architecture (`-arch arm64` / `-arch x64`), platform, installer schema version, and `%ProgramFiles64Folder%`.
- **JSON Template Driven**: MSI properties, GUIDs, shortcuts, and metadata are parameterized via JSON templates in `Templates/`.
- **Automatic Icon & Version Resolution**: Extracts version info directly from the binary and embeds product icons into Windows Add/Remove Programs (ARP).

---

## Quick Start

```powershell
# Build the MSI Creator tool
dotnet build App.MsiCreator/App.MsiCreator.csproj -c Release

# Create an MSI package
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t Templates/geteduroam/geteduroam-installer.json -e <path-to-geteduroam.exe>
```

For complete documentation, template schemas, and multi-architecture build scripts, see **[doc/MSICreator.md](../doc/MSICreator.md)**.
