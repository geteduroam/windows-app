# App.Library

`App.Library` is the shared WPF MVVM, UI presentation, and platform services library for `Eduroam.App` (`geteduroam`) and `Govroam.App` (`getgovroam`).

- **Target Framework**: `net8.0-windows10.0.19041.0` (WPF Class Library)
- **Language Version**: C# latest / .NET 8.0 LTS

---

## Directory & Component Structure

```
App.Library/
├── Binding/                - WPF XAML binding extensions and markup helpers
├── Command/                - Async RelayCommand and UI event command bindings
├── Connections/            - Wi-Fi profile configuration workflows (EAP, OAuth, Passphrase)
├── Converters/             - Value converters for XAML bindings (BooleanToVisibility, etc.)
├── Images/                 - Static branding assets and dynamic image loaders
├── Install/                - SelfInstaller for local AppData deployment & ARP registry entries
├── Styling/                - Shared WPF styles, brushes, control templates (Styling.xaml)
├── Templates/              - Page XAML DataTemplates mapping views to ViewModels
├── Utility/                - COM Interop, ArchitectureHelper, System Helpers
└── ViewModels/             - MVVM ViewModels handling business logic and navigation
```

---

## Key Modules & Platform Features

### 1. Pure C# COM Interop (`Utility/ComInterop.cs`)

Replaces legacy `tlbimp.exe` / `<COMReference>` bindings to eliminate `MSB4803` warnings and support cross-compilation across x64 and ARM64:

- **`IWshRuntimeLibrary`**: Pure C# COM interface declarations (`IWshShell`, `IWshShortcut`, `WshShellClass` with GUID `{F935DC21-1CF0-11D0-ADB9-00C04FD58A0B}`) to create and update Windows Start Menu and Desktop shortcuts.
- **`NETWORKLIST`**: Pure C# COM interface declarations (`INetworkListManager`, `NetworkListManagerClass` with GUID `{DCB00000-570F-4A9B-8D69-199FDBA5723B}`) to query real-time internet and network connection status.

### 2. Native Architecture Verification (`Utility/ArchitectureHelper.cs`)

Provides runtime hardware and binary architecture inspection using native Windows APIs:

- **PE Header Inspection (`GetFileArch`)**: Reads PE machine types directly from file headers:
  - `0xAA64` -> `MachineType.ARM64`
  - `0x8664` -> `MachineType.AMD64` (x64)
  - `0x014c` -> `MachineType.I386` (x86)
- **Native Execution Lock (`ProcessIsNative`, `GetNativeArch`, `GetProcessArch`)**: Invokes Win32 `IsWow64Process2` to determine host and process architecture, ensuring applications execute natively rather than under emulation.

### 3. MVVM UI Architecture

- **`MainWindow.xaml`**: Root window hosting dynamic `ContentPresenter` bound to current ViewModel.
- **`Templates/*`**: XAML DataTemplates binding each ViewModel to its visual layout:
  ```xml
  <DataTemplate DataType="{x:Type viewModels:SelectInstitutionViewModel}">
      <templates:SelectInstitutionControl />
  </DataTemplate>
  ```
- **`Styling/Styling.xaml`**: Central styling dictionary defining base colors, fonts, geometry, and button templates.

### 4. Self-Installation & Background Maintenance

- **`Install/SelfInstaller.cs`**: Deploys the application to `%LOCALAPPDATA%\geteduroam` (when running standalone) and writes Windows Add/Remove Programs uninstall entries.
- **Background Task Scheduling**: Integrates with `TaskScheduler` (David Hall) to register periodic background checks for institutional profile validity and certificate renewal.

---

## Localization Integration

`App.Library` consumes shared localization resources from `EduRoam.Localization` and dynamically inherits application-specific branding and resources from the entry applications (`Eduroam.App` or `Govroam.App`).