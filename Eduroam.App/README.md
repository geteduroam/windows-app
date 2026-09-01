# Eduroam.App

`Eduroam.App` is the primary WPF desktop application producing `geteduroam.exe`.

- **Target Framework**: `net8.0-windows10.0.19041.0` (WPF Executable)
- **Supported Runtime Identifiers**: `win-x64`, `win-arm64`
- **Output Executable**: `geteduroam.exe`

---

## Project Structure

- **`App.xaml` / `App.xaml.cs`**: Application entry point and bootstrapping:
  - Registers `System.Text.CodePagesEncodingProvider` for Cyrillic and international character encoding support.
  - Initializes the dependency injection container (`IServiceProvider`) via `Microsoft.Extensions.DependencyInjection`.
  - Configures `App.Settings.Settings` constants (OAuth client ID `app.geteduroam.win`, Discovery URL `https://discovery.eduroam.app/v3/discovery.json`, help URL `https://www.eduroam.app/`).
  - Sets up UI culture from `CultureInfo.CurrentUICulture`.
  - Launches `MainWindow` hosting the MVVM presentation pipeline.
- **`Styling/Styling.xaml`**: Eduroam-branded colors, themes, brushes, and asset resources.
- **`Resources.resx`**: Eduroam-specific strings, URLs, and localized titles.
- **`logo.png` & `geteduroam.ico`**: Official eduroam branding assets embedded into the executable and MSI installer.

---

## Building & Publishing

### Standard Build
```powershell
dotnet build Eduroam.App/Eduroam.App.csproj -c Release
```

### Single-File Self-Contained Publishing

#### 64-bit Intel/AMD (`win-x64`):
```powershell
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64/geteduroam
```

#### 64-bit ARM (`win-arm64`):
```powershell
dotnet publish Eduroam.App/Eduroam.App.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-arm64/geteduroam
```