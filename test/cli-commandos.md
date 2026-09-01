# CLI Commands & Test Matrix

This guide provides an exhaustive reference and test command matrix for validating `eduroam-cli.exe` / `geteduroam-cli.exe`.

---

## Command Help & Overview

To display all available commands and global options:
```powershell
eduroam-cli --help
```

For help on a specific subcommand:
```powershell
eduroam-cli <subcommand> --help
```

---

## Visual Studio Debugging Configuration

When debugging via Visual Studio (`EduRoam.CLI/Properties/launchSettings.json`), configure test arguments inside `"commandLineArgs"`:

```json
{
  "profiles": {
    "EduRoam.CLI": {
      "commandName": "Project",
      "commandLineArgs": "status"
    }
  }
}
```

---

## CLI Test Command Matrix

### 1. Institution & Profile Discovery (`list`)

| Scenario | Command Line | Expected Outcome |
|---|---|---|
| Search institutions by query | `eduroam-cli list -q "Moreelsepark"` | Returns list of matching institutions |
| Search institutions with spaces | `eduroam-cli list -q "University of"` | Returns matching institutions |
| List profiles for an institution | `eduroam-cli list -i "Moreelsepark College"` | Returns list of profiles (e.g. `Mijn Moreelsepark`) |
| List profiles for Uninett | `eduroam-cli list -i "uninett"` | Returns available profiles (e.g. `Ansatt`, `geteduroam (sertifikat)`) |
| List profiles for eduroam USA | `eduroam-cli list -i "eduroam USA"` | Returns `eduroam USA` profile |

### 2. Profile Details Inspection (`show`)

| Scenario | Command Line | Expected Outcome |
|---|---|---|
| Show profile details (credentials) | `eduroam-cli show -i "Moreelsepark College" -p "Mijn Moreelsepark"` | Displays auth method, realm, SSIDs |
| Show profile details (certificate) | `eduroam-cli show -i "uninett" -p "Ansatt"` | Displays certificate requirements and expiration |
| Show profile details (US roaming) | `eduroam-cli show -i "eduroam USA" -p "eduroam USA"` | Displays EAP-TLS / Passpoint configuration |
| Show profile from local file | `eduroam-cli show -c "test/templates/credentials.eap-config"` | Parses and displays local XML configuration |

### 3. Provisioning & Connection (`connect`)

| Scenario | Command Line | Expected Outcome |
|---|---|---|
| Username/Password connection | `eduroam-cli connect -i "Moreelsepark College" -p "Mijn Moreelsepark"` | Prompts for credentials, installs WLAN profile |
| Uninett employee connection | `eduroam-cli connect -i "uninett" -p "Ansatt"` | Configures 802.1X profile |
| Uninett certificate connection | `eduroam-cli connect -i "uninett" -p "geteduroam (sertifikat)"` | Prompts for cert passphrase, installs cert & profile |
| Client cert file connection | `eduroam-cli connect -i "eduroam USA" -p "eduroam USA" -cp "C:\Temp\geteduroam\geteduroam-test-cert.pfx"` | Installs client PFX certificate and associates with WLAN |
| Connect from local XML file | `eduroam-cli connect -c "test/templates/credentials.eap-config"` | Configures profile directly from `.eap-config` |
| Force re-configuration | `eduroam-cli connect -i "Moreelsepark College" -p "Mijn Moreelsepark" -f` | Overwrites existing profile cleanly |

### 4. Status, Maintenance & Lifecycle

| Scenario | Command Line | Expected Outcome |
|---|---|---|
| Query active connection status | `eduroam-cli status` | Displays active WLAN profile, SSID, and certificate validity |
| Refresh active profile & certs | `eduroam-cli refresh` | Re-queries CAT API and updates client certificates |
| Remove configured profile | `eduroam-cli remove` | Deletes WLAN profile from Windows Native Wi-Fi |
| Local self-installation | `eduroam-cli install` | Copies binary to `%LOCALAPPDATA%\geteduroam` and registers Task Scheduler job |
| Full uninstallation & cleanup | `eduroam-cli uninstall` | Unregisters scheduled task, removes WLAN profiles, and deletes CA/client certificates |