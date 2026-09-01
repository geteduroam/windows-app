# geteduroam / getgovroam for Windows

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20(x64%20%26%20ARM64)-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-BSD%203--Clause-blue.svg)](LICENSE.md)

This application configures **eduroam** and **govroam** wireless networks on Windows end-user devices by securely communicating with the CAT/geteduroam discovery API, parsing `.eap-config` profiles, installing identity certificates, and provisioning native Windows WLAN profiles.

---

## Architecture & Features

The solution is built on **.NET 8.0 LTS** (`net8.0-windows10.0.19041.0`) with dual-architecture native compilation:

- **Dual Native Architecture**: Native 64-bit binaries for **x64 (`win-x64`)** and **ARM64 (`win-arm64`)**, ensuring native execution on Intel/AMD systems as well as Qualcomm Snapdragon X Elite/Plus and Surface Pro devices with zero emulation overhead.
- **Single-File Self-Contained Bundling**: Native .NET 8 single-file publishing (`PublishSingleFile=true`) bundling all assemblies, WPF dependencies, and native C++ runtimes into a zero-dependency standalone executable.
- **Pure C# COM Interop**: Native COM interface definitions (`IWshRuntimeLibrary` and `NETWORKLIST`) in pure C# (`ComInterop.cs`), replacing legacy MSBuild `tlbimp.exe` type library wrappers and preventing `MSB4803` warnings across cross-architecture targets.
- **PE Machine Type Detection & WiX v4 Packaging**: Automated PE binary inspection in `App.MsiCreator` detecting `0xAA64` (ARM64) and `0x8664` (x64) machine types to generate native 64-bit WiX v4 Windows Installer (`.msi`) packages targeting `%ProgramFiles64Folder%`.
- **Cyrillic & International Encoding Support**: Registered `CodePagesEncodingProvider` (`System.Text.Encoding.CodePages`) with Unicode fallback normalization to handle institutional names and Wi-Fi profiles across all international character sets.

---

## Verified Hardware & Test Environment

Native ARM64 build and execution have been tested and verified on physical Windows on ARM hardware:

| Environment | Details |
|---|---|
| **Device Model** | Microsoft Surface Pro (11th Edition, Copilot+ PC) |
| **Processor** | Qualcomm Snapdragon® X Plus 10-core (X1P64100 @ 3.40 GHz) |
| **Architecture** | Native 64-bit ARM (`ARM64` / `win-arm64`) |
| **Operating System** | Windows 11 Pro (Build 26220.x, ARM64) |
| **Tested Runtimes** | .NET 8.0, .NET 9.0 (`Microsoft.WindowsDesktop.App` 9.0.12), .NET 10.0 (`10.0.9`) |

---

## Solution Structure

The solution `EduroamApp.sln` contains 8 active projects:

| Project | Target Framework | Output | Description |
|---|---|---|---|
| **`Eduroam.App`** | `net8.0-windows10.0.19041.0` | `geteduroam.exe` | Main WPF GUI client for eduroam |
| **`Govroam.App`** | `net8.0-windows10.0.19041.0` | `getgovroam.exe` | Branded WPF GUI client for govroam |
| **`EduRoam.CLI`** | `net8.0-windows10.0.19041.0` | `eduroam-cli.exe` | Headless, scriptable command-line interface |
| **`App.Library`** | `net8.0-windows10.0.19041.0` (WPF) | `App.Library.dll` | Shared MVVM UI layer, ViewModels, COM interop, `ArchitectureHelper` |
| **`EduRoam.Connect`** | `net8.0-windows10.0.19041.0;netstandard2.0` | `EduRoam.Connect.dll` | WLAN profile provisioning, EAP configuration engine, `ManagedNativeWifi` |
| **`EduRoam.Localization`**| `net8.0-windows10.0.19041.0;netstandard2.0` | `EduRoam.Localization.dll` | Multi-language ResX resources (20+ localized languages) |
| **`App.Settings`** | `net8.0-windows10.0.19041.0;netstandard2.0` | `App.Settings.dll` | Shared application configuration and discovery constants |
| **`App.MsiCreator`** | `net8.0-windows10.0.19041.0` | `App.MsiCreator.exe` | WiX v4 / WixSharp automated MSI installer generator |

---

## Supported Authentication Modes

The following EAP and roaming methods can be fully configured:

- **PEAP-MSCHAPv2** *(Note: On PEAP-MSCHAPv2, the OuterIdentity must match the realm of the username due to Windows networking constraints)*
- **TLS** (Client Certificates)
- **TTLS-PAP**
- **TTLS-MSCHAP**
- **TTLS-MSCHAPv2**
- **TTLS-EAP-MSCHAPv2**
- **Hotspot 2.0 / Passpoint** profiles for all supported EAP types

---

## Installation & Lifecycle

- **Local Self-Installation**: When run standalone, after configuring a profile, geteduroam installs itself to `%LOCALAPPDATA%\geteduroam` (or `%ProgramFiles64Folder%\geteduroam` when installed via MSI).
- **Control Panel Integration**: Adds uninstaller entries to Windows Settings / Control Panel (*Add/Remove Programs*) to cleanly roll back network profiles and certificates upon uninstall.
- **Certificate Renewal Scheduling**: Registers a background task in Windows Task Scheduler to periodically verify certificate expiration and prompt for renewal.

---

## Getting Started

### Prerequisites

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** (v8.0.100 or later)
- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** (v17.8 or later) with the **.NET Desktop Development** workload, or **VS Code** / CLI.
- **Operating System**: Windows 10 (version 19041.0 / 2004 or later) or Windows 11 (x64 and ARM64).

### Building the Solution

Clone the repository and build the entire solution using the .NET CLI:

```powershell
# Restore dependencies
dotnet restore EduroamApp.sln

# Build all projects in Release configuration
dotnet build EduroamApp.sln -c Release --no-restore

# Run unit tests
dotnet test EduroamApp.sln -c Release --no-build
```

---

## Publishing Single-File Executables

To create self-contained, zero-dependency single-file binaries, use `dotnet publish` with the desired Runtime Identifier (`-r win-x64` or `-r win-arm64`):

### 64-bit Intel/AMD (`win-x64`)

```powershell
# geteduroam GUI
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/geteduroam

# getgovroam GUI
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/getgovroam

# eduroam CLI
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/eduroam-cli
```

### 64-bit ARM (`win-arm64`)

```powershell
# geteduroam GUI
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/geteduroam

# getgovroam GUI
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/getgovroam

# eduroam CLI
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/eduroam-cli
```

---

## Creating WiX v4 MSI Installers

`App.MsiCreator` packages published single-file executables into native Windows Installer (`.msi`) packages using **WiX v4** (`WixSharp_wix4.bin`). It automatically inspects the target PE binary's machine type (`0xAA64` for ARM64 vs `0x8664` for x64) to set the appropriate installer platform and options.

```powershell
# Build App.MsiCreator
dotnet build App.MsiCreator/App.MsiCreator.csproj -c Release

# Create geteduroam MSI (x64)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam/geteduroam-installer.json -e publish/win-x64/geteduroam/geteduroam.exe

# Create geteduroam MSI (ARM64)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam/geteduroam-installer.json -e publish/win-arm64/geteduroam/geteduroam.exe
```

For detailed template options and configuration, see [doc/MSICreator.md](doc/MSICreator.md).

---

## Code Signing

When deploying production builds, sign executables and MSI installers using a valid code signing certificate:

```powershell
# Using Microsoft Windows SDK signtool.exe
signtool.exe sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a "publish/win-x64/geteduroam/geteduroam.exe"
signtool.exe sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a "geteduroam.msi"
```

---

## Third-Party Dependencies

All dependencies are managed via NuGet:

- **[DuoVia.FuzzyStrings](https://github.com/tylerjensen/DuoVia.FuzzyStrings)** by Tyler Jensen ~ Apache-2.0
- **[ManagedNativeWifi](https://github.com/emoacht/ManagedNativeWifi)** by emoacht ~ [LICENSE](Licenses/ManagedNativeWifi_LICENSE.md)
- **[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)** by James Newton-King ~ [LICENSE](Licenses/Newtonsoft.Json_LICENSE.md)
- **[System.CommandLine](https://github.com/dotnet/command-line-api)** by .NET Foundation ~ MIT License
- **[System.Text.Encoding.CodePages](https://dot.net/)** by .NET Foundation ~ MIT License
- **[TaskScheduler](https://github.com/dahall/taskscheduler)** by David Hall ~ MIT License
- **[WixSharp_wix4.bin](https://github.com/oleg-shilo/wixsharp)** by Oleg Shilo ~ MIT License
- **[NLog.Extensions.Logging](https://github.com/NLog/NLog)** by NLog Team ~ BSD-3-Clause
- **[Microsoft.Toolkit.Uwp.Notifications](https://github.com/CommunityToolkit/WindowsCommunityToolkit)** ~ MIT License
- **[Semver](https://github.com/maxhauser/semver)** by Max Hauser ~ MIT License

---

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE.md](LICENSE.md) file for details.
