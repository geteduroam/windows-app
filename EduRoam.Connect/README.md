# EduRoam.Connect

`EduRoam.Connect` is the core wireless networking, discovery, and profile configuration engine for geteduroam and getgovroam.

- **Target Frameworks**: `net8.0-windows10.0.19041.0;netstandard2.0`
- **Language Version**: C# latest / .NET 8.0 LTS

---

## Core Responsibilities

1. **CAT / geteduroam Discovery API Client**:
   - Queries institutional discovery endpoints (e.g., `https://discovery.eduroam.app/v3/discovery.json`).
   - Parses and indexes identity providers (IdPs), profiles, logos, and authentication endpoints.
   - Performs fuzzy search over institution names using `DuoVia.FuzzyStrings`.

2. **EAP-Config XML Parser**:
   - Parses standard IEEE 802.1X `.eap-config` XML payloads.
   - Extracts server certificate authorities, client certificate requirements, authentication method hierarchies, inner/outer identities, and SSID/Hotspot 2.0 configurations.

3. **WLAN Profile Provisioning via `ManagedNativeWifi`**:
   - Interfaces with Windows Native Wi-Fi (`wlanapi.dll`) via `ManagedNativeWifi 2.5.0`.
   - Generates Windows Native Wi-Fi XML schemas (`WLANProfile`, `OneX`, `MSM`, `EapHostUserCredentials`).
   - Deploys profiles and associates user/client credentials with targeted network interfaces.

4. **Certificate & Trust Management**:
   - Installs Root and Intermediate Certificate Authorities (CAs) into the Windows Certificate Store (`Cert:\CurrentUser\Root`, `Cert:\CurrentUser\CA`).
   - Provisions client certificates (PKCS#12 / `.pfx`) into `Cert:\CurrentUser\My` for EAP-TLS authentication.
   - Manages clean removal of certificates upon uninstallation or profile refresh.

5. **Internationalization & Cyrillic Encoding**:
   - Automatically registers `System.Text.CodePagesEncodingProvider` via `System.Text.Encoding.CodePages`.
   - Supports Windows-1251 (Cyrillic) and ISO-8859 encodings in institutional names and profile metadata.
   - Implements Unicode `NormalizationForm.FormD` diacritic-stripping fallback for resilient institution search.

---

## Supported EAP & Roaming Methods

| Authentication Protocol | Inner Method | Credential Type | Notes |
|---|---|---|---|
| **PEAP** | MSCHAPv2 | Username / Password | Outer Identity realm must match username realm (Windows requirement) |
| **TLS** | None (Certificate) | Client Certificate (`.pfx`) | Automated certificate generation or user-provided PKCS#12 |
| **TTLS** | PAP | Username / Password | Standard EAP-TTLS tunnel |
| **TTLS** | MSCHAP | Username / Password | Challenge handshake |
| **TTLS** | MSCHAPv2 | Username / Password | Mutual challenge handshake |
| **TTLS** | EAP-MSCHAPv2 | Username / Password | EAP within TTLS tunnel |
| **Hotspot 2.0 / Passpoint**| Supported EAP | Credentials / Certs | Deploys Passpoint network profiles |

---

## Diagnostics & Troubleshooting

When troubleshooting Wi-Fi configuration or authentication issues on Windows:

### 1. Generate Windows WLAN Report
Open an elevated Command Prompt or PowerShell:
```powershell
netsh wlan show wlanreport
```
*Output*: A detailed HTML report is generated at `C:\ProgramData\Microsoft\Windows\WlanReport\wlan-report-latest.html`.

### 2. Inspect Configured WLAN Profiles
```powershell
# List all configured Wi-Fi profiles
netsh wlan show profiles

# Inspect specific profile details (including EAP settings)
netsh wlan show profile name="eduroam" key=clear

# Inspect wireless interface capabilities and drivers
netsh wlan show interfaces
netsh wlan show drivers
```

### 3. Windows Event Viewer Channels
- **EAP / 802.1X Events**: `Applications and Services Logs > Microsoft > Windows > EapMethods-RasTls / EapHost`
- **WLAN AutoConfig Events**: `Applications and Services Logs > Microsoft > Windows > WLAN-AutoConfig > Operational`

---

## Technical References

- [Windows WLAN Profile Schema](https://learn.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-elements)
- [OneX EAP Configuration Schema](https://learn.microsoft.com/en-us/windows/win32/nativewifi/onexschema-schema)
- [EAPHost User Credentials Schema](https://learn.microsoft.com/en-us/windows/win32/eaphost/eaphostusercredentialsschema-schema)
- [Configure 802.1X EAP Profiles in Windows](https://learn.microsoft.com/en-us/windows-server/networking/technologies/extensible-authentication-protocol/configure-eap-profiles)