# App.Settings

`App.Settings` is a lightweight shared configuration library that encapsulates global runtime constants, discovery endpoints, OAuth parameters, and branding properties for geteduroam and getgovroam.

- **Target Frameworks**: `net8.0-windows10.0.19041.0;netstandard2.0`
- **Language Version**: C# latest / .NET 8.0 LTS

---

## Configuration Properties

The static `App.Settings.Settings` class provides the following application parameters:

| Property | Type | Default (eduroam) | Description |
|---|---|---|---|
| `OAuthClientId` | `string` | `"app.geteduroam.win"` | OAuth 2.0 client identifier used during institutional authorization flows. |
| `ApplicationName` | `string` | `"geteduroam"` | Application process and display name. |
| `NetworkName` | `string` | `"eduroam"` | Primary SSID / network name to configure. |
| `Publisher` | `string` | `"SURF"` | Software publisher string written to registry and shortcut metadata. |
| `UpdateBaseUrl` | `string` | `"https://dl.eduroam.app"` | Base URL used to query for application updates. |
| `DaysLeftForNotification` | `int` | `10` | Days remaining before certificate expiration when user notifications trigger. |
| `EapConfigFileLocation` | `string?` | `null` | Optional path to pre-bundled local `.eap-config` file for standalone deployments. |
| `HelpUrl` | `string` | `"https://www.eduroam.app/"` | Institutional / technical support website. |
| `BrowserDownloadUrl` | `string` | `"https://www.eduroam.app/"` | Web URL for installer downloads. |
| `DiscoveryUrl` | `string` | `"https://discovery.eduroam.app/v3/discovery.json"` | JSON discovery endpoint listing identity providers and profiles. |

---

## Architecture Customization

When initializing `Govroam.App` or customized builds, these static values are re-assigned at application startup (`App.xaml.cs`) to dynamically point to the appropriate branding, discovery, and OAuth infrastructure.
