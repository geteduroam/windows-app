# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.0.0] - Unreleased

### Added
* **Native ARM64 Architecture Support**: Full native compilation and single-file publishing for ARM64 (`win-arm64`) alongside 64-bit Intel/AMD (`win-x64`), providing native execution on Surface Pro 9/11, Snapdragon X Elite/Plus devices, and Windows Dev Kit with zero emulation overhead.
* **Pure C# COM Interop (`ComInterop.cs`)**: Replaced legacy MSBuild `tlbimp.exe` / `<COMReference>` bindings with pure C# COM interfaces decorated with `[ComImport]` and `[Guid]` for `IWshRuntimeLibrary` (Windows Script Host shortcuts) and `NETWORKLIST` (network connectivity manager), eliminating `MSB4803` warnings and enabling seamless cross-architecture compilation.
* **WiX v4 Installer Engine & Architecture Auto-Detection**: Modernized `App.MsiCreator` to use `WixSharp_wix4.bin` 2.14.1 (WiX v4) with automatic PE header binary inspection (`GetPeMachineType`), detecting `0xAA64` (ARM64) and `0x8664` (x64) binaries to configure native 64-bit platforms, Windows Installer 5.0 (for ARM64), and installation paths targeting `%ProgramFiles64Folder%`.
* **Native Architecture Lock (`ArchitectureHelper`)**: Added `ArchitectureHelper` utility using `IsWow64Process2` and PE header inspection to verify native process execution and prevent accidental emulated execution.
* **Cyrillic & International Encoding Support**: Integrated `System.Text.Encoding.CodePages` and registered `CodePagesEncodingProvider` across GUI and CLI startup routines (`IdentityProviderParser`, `Eduroam.App`, `Govroam.App`, `EduRoam.CLI`), resolving crashes when parsing institutional profiles containing non-ASCII / Cyrillic (Windows-1251) characters. Added Unicode `NormalizationForm.FormD` diacritic-stripping fallback for robust string search.
* **Automated CI/CD Pipeline**: Introduced GitHub Actions workflow (`.github/workflows/build.yml`) automating multi-architecture restore, compilation, single-file self-contained publishing, WiX v4 MSI generation, and release artifact archiving across `win-x64` and `win-arm64`.

### Changed
* **Target Framework Upgrade**: Migrated all active projects to **.NET 8.0 LTS** (`net8.0-windows10.0.19041.0` and `netstandard2.0`).
* **Native Single-File Publishing**: Replaced `Costura.Fody` IL weaving with native .NET 8 single-file self-contained publishing (`PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true`, `EnableCompressionInSingleFile=true`).
* **MVVM Architecture Refactoring**: Modernized WPF presentation layer (`App.Library`) using structured MVVM patterns, data templates, and dependency injection (`Microsoft.Extensions.DependencyInjection` 8.0.0).
* **Logging Modernization**: Upgraded logging infrastructure to `NLog.Extensions.Logging` 5.3.8 and `Microsoft.Extensions.Logging` 8.0.0.
* **Dependency Updates**: Updated `ManagedNativeWifi` (2.5.0), `Newtonsoft.Json` (13.0.3), `System.CommandLine` (2.0.0-beta4), `Semver` (2.3.0), and `TaskScheduler` (2.10.1).

### Removed
* **Costura.Fody Elimination**: Removed `Costura.Fody`, `Fody`, and `FodyWeavers.xml` configuration files across all projects.
* **Legacy Projects**: Removed deprecated `EduroamConfigure` and `WpfApp` legacy prototypes from the active solution.
* **Geolocation Module**: Removed legacy geolocation subsystem in favor of fast, cached CAT discovery API querying.
* **Legacy COM TypeLib Dependencies**: Removed obsolete COM type library wrapper dependencies.

### Fixed
* Fixed crash on startup/search when parsing institutional names with Cyrillic or non-Unicode encodings.
* Fixed Windows Script Host COM shortcut creation on ARM64 devices.
* Fixed missing ARM64 Add/Remove Programs registry metadata during MSI installation.

## [geteduroam 3.2.10](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.10)

### Fixed
* NullPointerException when OuterIdentity is not set

### Changed
* Updated ManagedNativeWifi, is now a NuGet package
* Updated NuGet packages
* Updated target to Visual Studio 2022
* Removed code for running in the background; this was never needed


## [geteduroam 3.2.9](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.9)

### Fixed
* Fix Back button not working after opening an .eap-config file manually
* Drop realm from OuterIdentity for PEAP-MSCHAPv2, as Windows adds the realm itself

#### Changed
* Handle discovery "seq" as string, not integer; this prevents a crash when seq cannot be parsed as integer
* Prevent disabling CA checking; this was not possible but now the code path is also removed
* Handle more network errors when loading the discovery file
* Remove nonsensical TTLS-EAP-PEAP-MSCHAPv2 method; there doesn't seem to be a server that supports this
* Send `Connection: Close` header when retrieving the discovery


## [geteduroam 3.2.8](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.8)

### Fixed

* Fix some devices failing with ErrorCode 1206


## [geteduroam 3.2.7](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.7)

**This version is production ready, but not yet released as a signed executable.**

You can continue to use geteduroam 3.2.6 for now; this version consists mainly of fixes under the hood.

### Fixed

* Prevent a hard crash when no Wi-Fi adapter is available

### Changed

* Refactors for simpler authentication flows


## [geteduroam 3.2.6](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.6)

### Fixed

* A bug where the outer ID was not set for PEAP (but only TTLS)


## [geteduroam 3.2.5.1](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.5.1)

### Changed

* User interface improvements
* Consistently use services from eduroam.app (not from the geteduroam.app)

### Fixed

* Prevent crash when pressing No on a certificate prompt

### Removed

* Unused images


## [geteduroam 3.2.5](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.5)

### User interface

* Hide advanced UI elements
* Focus on important UI elements
* Support SVG logos
* Allow uninstalling from within the UI

### Wi-Fi and eduroam settings

* Remove authentication method filter for HS20
* (Windows supports the same methods for SSIDs and HS20)
* Configure Wi-Fi network as non-hidden
* Identify institution by id field instead of cat_idp field

### Stability

* Prevent random crash obtaining the version number
* Geolocation and discovery download can be done in parallel
* Retry discovery download if failed earlier
* Fix a bug where the geo web API was used more than once
* Prevent calling geo web API if the local geo API has a result

### Code climate

* Replace WebClient with the newer HttpClient
* Use single HttpClient instance for resource reuse
* Groundwork for better logging in a later release
* Groundwork for using WifiManager as external dependency
* Code cleanup, prevent some edge-case bugs
* Code cleanup, fix warnings

### Known bugs

* Cannot write a log file
* Cannot configure a network for all users


## [geteduroam 3.2.4](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.4)

* Shorten the time between creating a Wi-Fi profile and adding user data to it, lowering the chance for a "enter your password" prompt from Windows.
* Prevent error code 57893 by refusing to configure TTLS-EAP-MSCHAPv2 for HS20 profiles. The profile can be made without problems, but adding user credentials to it causes the error code. TTLS-MSCHAPv2 seems to work fine, so we keep that on for now.
* Add User Agent to the HTTP client so we can keep better statistics.


## [geteduroam 3.2.3](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.3)

This is a bugfix release, where problems with Passpoint are ignored if SSID configuration succeeds.
We have made this change because it appears that some hardware does not support Passpoint, but the error returned by the Windows API seems to not be consistent across hardware.

Because of this, we cannot be sure if a problem configuring Passpoint is due to lacking hardware support, or due to a problem with the profile. The assumption is therefore that, as long as at least one SSID was configured, the problem is lacking hardware support.

* Ignore all HS20 errors if SSID configured


## [geteduroam 3.2.2](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.2)

* Show better error messages
* Use correct HTTP method for pseudo-credential
* Ignore Hotspot 2.0 errors if SSID configuration succeeded
* Fail on certificate errors during connection, do not prompt user


## [geteduroam 3.2.1](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.1)

* Prevent crashes when attempting to refresh a non-CAT profile
* Update username suggestions to support eap-config files with empty realm requirement (coming soon to CAT)


## [geteduroam 3.2.0](https://github.com/geteduroam/windows-app/releases/tag/geteduroam-3.2.0)

* During installation, do not copy NTFS attributes, so the installed .exe file has no [Zone.Identifier](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/6e3f7352-d11c-4d76-8c39-2516a9df36e8) stream.
* Show confirmation after uninstall
* Set geteduroam on top over browser window after completing OAuth challenge
* Disable use of tray icon
* Prevent crash when no network connection is available
* Prevent uninstall failure when no profiles are installed
* Install geteduroam into Windows when a change is made to the system (so the uninstaller can roll it back)
* Improved GUI
	* Seamless header
	* Larger fonts
	* Sharp edges (disable anti-aliasing for square components)
	* Disabled buttons are lighter
	* Resizeable window (only vertical for now)
	* Better focus flow (use tab button)
	* Pressing ESC or the physical back button (only some keyboards) will take you to the previous screen
	* When searching an institute, the first match is automatically marked so you can press Enter to use it
	* Pressing up/down in institute search will move the marker in the listbox
	* Disable Next button on institute search when no institute is selected
	* Rightclick on contact details to copy them
	* Switch focus between username/password fields with arrow keys
	* Pressing enter in the username field will focus the password field
* Many cleanups


## [geteduroam 3.1.2](https://github.com/geteduroam/windows-app/releases/tag/v3.1.2)

* Rename 'refresh' to 'refresh now'
* Disable 'refresh now' button while refreshing
* Cleanup client certificates on refresh


## [geteduroam 3.1.1](https://github.com/geteduroam/windows-app/releases/tag/v3.1.1)

* OAuth cancelling no longer crashes the application
* Refresh button on Installed Profile page for OAuth refresh

### Known issues

* Refresh button not disabled while refreshing on InstalledProfile page


## [geteduroam 3.1.0](https://github.com/geteduroam/windows-app/releases/tag/v3.1.0.0)

### Changes

* Page showing currently configured profile
* UI improvements
* Force users to remove root CAs when uninstalling the application
* plenty of bug fixes

### Issues

* Missing UI for uninstalling root CA inside the application
* Cancelled OAuth session causes crash
* No button to refresh OAuth token


## [geteduroam 3.0.1](https://github.com/geteduroam/windows-app/releases/tag/V3RC2)

Second release candidate with the Wpf GUI and installation

### Notable changes

* More understandable institution browser
* Improved navigation
* Support non-svg logos
* CLI interface

### Known issues

* Does not update certificate yet
* Still no overview of the currently installed profile (if it exists)


## [geteduroam 3.0.0](https://github.com/geteduroam/windows-app/releases/tag/V3RC1)

First release candidate with the Wpf GUI and installation

### Notable changes

* geteduroam can install itself in AppData with the /install flag
* geteduroam can remove all changes it made with the /uninstall flag
* Use the new Wpf GUI framework
* Stores refresh_token in registry to update certificate later
* Many bugfixes

### Known issues

* Application shows tray icon on first start, should only show when installed
* Clicking the tray icon before the UI shows may crash the application
* Uninstall silently fails if tray icon still is running
* Does not update certificate yet
* No longer compatible with the old PoC lets-wifi implementation (will not fix, works with the newer letswifi-ca implementation
