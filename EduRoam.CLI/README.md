# EduRoam.CLI

`EduRoam.CLI` is the headless, scriptable command-line interface producing `eduroam-cli.exe`. It allows automated provisioning, profile inspection, connection status verification, and uninstallation of eduroam network profiles in enterprise and scripted environments.

- **Target Framework**: `net8.0-windows10.0.19041.0` (Console Application)
- **Supported Runtime Identifiers**: `win-x64`, `win-arm64`
- **Output Executable**: `eduroam-cli.exe`

---

## Command Reference

The CLI is built using `System.CommandLine`. Run `eduroam-cli --help` for full inline command help.

### 1. `list` — Search Institutions or Profiles
```powershell
# Search institutions matching a keyword
eduroam-cli list -q "University"

# List available profiles for a specific institution
eduroam-cli list -i "University of Example"
```

### 2. `show` — Display Profile Details
```powershell
# Show profile configuration details for an institution
eduroam-cli show -i "University of Example" -p "Staff & Students"

# Show profile configuration from a local .eap-config file
eduroam-cli show -c "C:\Path\To\profile.eap-config"
```

### 3. `connect` — Provision and Connect to eduroam
```powershell
# Connect using institution and profile name (prompts for credentials if needed)
eduroam-cli connect -i "University of Example" -p "Staff & Students"

# Connect from local .eap-config file
eduroam-cli connect -c "C:\Path\To\profile.eap-config"

# Connect using client certificate file (.pfx)
eduroam-cli connect -i "University of Example" -p "Client Certificate" -cp "C:\Certs\user.pfx"

# Force profile re-installation
eduroam-cli connect -i "University of Example" -p "Staff & Students" --force
```

### 4. `status` — View Connection & Profile Status
```powershell
eduroam-cli status
```

### 5. `refresh` — Refresh Configured Profile & Certificates
```powershell
eduroam-cli refresh
```

### 6. `remove` — Remove Configured Network Profile
```powershell
eduroam-cli remove
```

### 7. `install` / `uninstall` — Application Lifecycle
```powershell
# Install CLI to local AppData and register Task Scheduler refresh job
eduroam-cli install

# Uninstall geteduroam, remove configured WLAN profiles, and delete installed certificates
eduroam-cli uninstall
```

---

## Building & Publishing

### Standard Build
```powershell
dotnet build EduRoam.CLI/EduRoam.CLI.csproj -c Release
```

### Single-File Self-Contained Publishing

#### 64-bit Intel/AMD (`win-x64`):
```powershell
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/eduroam-cli
```

#### 64-bit ARM (`win-arm64`):
```powershell
dotnet publish EduRoam.CLI/EduRoam.CLI.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/eduroam-cli
```
