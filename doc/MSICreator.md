# WiX v4 MSI Installer Creator Guide

`App.MsiCreator` is a dedicated .NET 8 console application (`net8.0-windows10.0.19041.0`) designed to package single-file self-contained executables (`geteduroam.exe`, `getgovroam.exe`, and `eduroam-cli.exe`) into enterprise-ready Windows Installer (`.msi`) packages using **WiX v4** and **WixSharp** (`WixSharp_wix4.bin` 2.14.1).

---

## Key Capabilities & Architecture

1. **Automated PE Header Architecture Detection**:
   `App.MsiCreator` inspects the target `.exe` binary's Portable Executable (PE) header via `GetPeMachineType` at runtime:
   - **ARM64 (`0xAA64` / `IMAGE_FILE_MACHINE_ARM64`)**:
     - Configures WixSharp `Platform = Platform.arm64`.
     - Appends compiler option `Compiler.WixOptions += " -arch arm64"`.
     - Sets `InstallerVersion = 500` (Windows Installer 5.0 required for native ARM64 MSI packages).
     - Targets the native 64-bit program files directory: `%ProgramFiles64Folder%`.
   - **x64 (`0x8664` / `IMAGE_FILE_MACHINE_AMD64`)**:
     - Configures WixSharp `Platform = Platform.x64`.
     - Appends compiler option `Compiler.WixOptions += " -arch x64"`.
     - Sets `InstallerVersion = 200`.
     - Targets the native 64-bit program files directory: `%ProgramFiles64Folder%`.
   - **x86 (`0x014C` / `IMAGE_FILE_MACHINE_I386` fallback)**:
     - Configures WixSharp `Platform = Platform.x86`.
     - Sets `InstallerVersion = 200`.
     - Targets standard `%ProgramFiles%`.

2. **Automated Version & Metadata Extraction**:
   - Reads the application version using `FileVersionInfo.GetVersionInfo(exePath)` and strips SemVer build metadata (e.g., `4.2.6+abc123` -> `4.2.6`).
   - Automatically resolves and embeds application icons (`.ico`) into the MSI package and registers the icon with Windows Add/Remove Programs (ARP).
   - Configures standard installation attributes (`NoModify = true`, progress-only UI, clean uninstall handlers).

---

## Command-Line Syntax

`App.MsiCreator` uses `System.CommandLine` with the `create` verb:

```powershell
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release -- create -t <template-json-path> -e <executable-path>
```

### Options

| Option | Alias | Type | Required | Description |
|---|---|---|---|---|
| `--template` | `-t` | `FileInfo` | Yes | Path to the JSON installer configuration template |
| `--exe` | `-e` | `FileInfo` | Yes | Path to the target single-file `.exe` to package |

---

## JSON Template Schema

Installer definitions are parameterized using JSON template files located in `App.MsiCreator/Templates/`:

```json
{
  "appTitle": "geteduroam",
  "programFolder": "geteduroam",
  "installerId": "971ad33e-ab1c-4697-a749-f6449be6bce8",
  "appIconPath": "geteduroam.ico",
  "manufacturer": "SURF"
}
```

### Field Definitions

| Field | Type | Description |
|---|---|---|
| `appTitle` | `string` | Display name of the product. Also determines the base name of the generated MSI (`<appTitle>.msi`). |
| `programFolder` | `string` | Name of the installation subdirectory created under `%ProgramFiles64Folder%` (e.g. `%ProgramFiles%\geteduroam`). |
| `installerId` | `string` | Unique GUID identifying the product code for Windows Installer version tracking and upgrades. |
| `appIconPath` | `string` | Relative or absolute path to the `.ico` icon file to embed into the package and Add/Remove Programs. |
| `manufacturer` | `string` | Organization or vendor name displayed in Windows Settings and Control Panel. |

### Available Templates

- `App.MsiCreator/Templates/geteduroam/geteduroam-installer.json` — geteduroam GUI installer
- `App.MsiCreator/Templates/getgovroam/getgovroam-installer.json` — getgovroam GUI installer
- `App.MsiCreator/Templates/geteduroam-cli/geteduroam-installer.json` — geteduroam CLI installer

---

## Step-by-Step Build & Packaging Workflow

### Step 1: Publish Single-File Executables

Publish the target application as a self-contained single-file binary for the desired architecture:

#### For 64-bit Intel/AMD (`win-x64`):
```powershell
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/geteduroam
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/getgovroam
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/eduroam-cli
```

#### For 64-bit ARM (`win-arm64`):
```powershell
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/geteduroam
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/getgovroam
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/eduroam-cli
```

---

### Step 2: Build `App.MsiCreator`

Compile the MSI builder tool:

```powershell
dotnet build App.MsiCreator/App.MsiCreator.csproj -c Release
```

---

### Step 3: Generate MSIs

Run `App.MsiCreator` pointing to the appropriate template and published `.exe`.

#### Building x64 MSIs:
```powershell
# geteduroam x64 MSI (produces geteduroam.msi)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam/geteduroam-installer.json -e publish/win-x64/geteduroam/geteduroam.exe

# getgovroam x64 MSI (produces getgovroam.msi)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/getgovroam/getgovroam-installer.json -e publish/win-x64/getgovroam/getgovroam.exe

# eduroam CLI x64 MSI (produces 'geteduroam cli.msi')
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam-cli/geteduroam-installer.json -e publish/win-x64/eduroam-cli/eduroam-cli.exe
```

#### Building ARM64 MSIs:
```powershell
# geteduroam ARM64 MSI (produces geteduroam.msi)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam/geteduroam-installer.json -e publish/win-arm64/geteduroam/geteduroam.exe

# getgovroam ARM64 MSI (produces getgovroam.msi)
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/getgovroam/getgovroam-installer.json -e publish/win-arm64/getgovroam/getgovroam.exe

# eduroam CLI ARM64 MSI (produces 'geteduroam cli.msi')
dotnet run --project App.MsiCreator/App.MsiCreator.csproj -c Release --no-build -- create -t App.MsiCreator/Templates/geteduroam-cli/geteduroam-installer.json -e publish/win-arm64/eduroam-cli/eduroam-cli.exe
```

---

## MSI Installation & Deployment

Generated MSIs can be deployed interactively or silently across enterprise managed devices:

```powershell
# Silent installation (installs to %ProgramFiles%\geteduroam)
msiexec.exe /i "geteduroam.msi" /qn /norestart

# Silent uninstallation
msiexec.exe /x "geteduroam.msi" /qn /norestart

# Installation with logging
msiexec.exe /i "geteduroam.msi" /qn /l*v "install.log"
```