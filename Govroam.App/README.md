# Govroam.App

`Govroam.App` is the branded WPF desktop application producing `getgovroam.exe`.

- **Target Framework**: `net8.0-windows10.0.19041.0` (WPF Executable)
- **Supported Runtime Identifiers**: `win-x64`, `win-arm64`
- **Output Executable**: `getgovroam.exe`

---

## Project Structure & Branding

`Govroam.App` shares the underlying architecture and MVVM components from `App.Library` while supplying govroam-specific branding, styling, and discovery configuration:

- **`App.xaml` / `App.xaml.cs`**: Application entry point and bootstrapping:
  - Registers `System.Text.CodePagesEncodingProvider`.
  - Initializes DI container (`Microsoft.Extensions.DependencyInjection`).
  - Configures `App.Settings.Settings` with govroam endpoints:
    - `OAuthClientId`: `"app.getgovroam.win"`
    - `ApplicationName`: `"getgovroam"`
    - `NetworkName`: `"govroam"`
    - `DiscoveryUrl`: `"https://discovery.govroam.app/v3/discovery.json"`
    - `HelpUrl`: `"https://www.govroam.app/"`
    - `UpdateBaseUrl`: `"https://dl.govroam.app"`
- **`Styling/Styling.xaml`**: Govroam green branding theme, brushes, and visual styles.
- **`Resources.resx`**: Govroam-specific localized text and support links.
- **`logo.png` & `getgovroam.ico`**: Official govroam visual assets.

---

## Building & Publishing

### Standard Build
```powershell
dotnet build Govroam.App/Govroam.App.csproj -c Release
```

### Single-File Self-Contained Publishing

#### 64-bit Intel/AMD (`win-x64`):
```powershell
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/getgovroam
```

#### 64-bit ARM (`win-arm64`):
```powershell
dotnet publish Govroam.App/Govroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/getgovroam
```