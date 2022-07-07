# eduroam app for Windows

This application helps set up eduroam on end-users' computers by automatically fetching and installing the required certificates.

## Structure

* **WpfApp**:           The graphical user interface and UX logic.
* **EduroamConfigure**: The logic interfacing with the discovery api, and the logic to parse and configure the various profiles into windows.

## Supported authentication modes

The following EAP methods can be fully configured:

* PEAP-MSCHAPv2
* TLS
* TTLS-PAP
* TTLS-MSCHAP
* TTLS-MSCHAPv2
* TTLS-EAP-MSCHAPv2

The only exception is that on PEAP-MSCHAPv2, the OuterIdentity must have the same realm as the username.
This is a limitation set by Windows.

For all modes, you can also install Hotspot 2.0.


## Installation

After a change to the system has been made, geteduroam will install itself to `%HOME%\AppData\Local\geteduroam`.
It will add itself to the registry to be listed in installed programs, how to uninstall it, and a task will be registered with
the task scheduler, which will prompt geteduroam check for updates on the profile.
A tray icon for running in the background can be enabled through a project flag, but it is disabled by default.


## Getting started

### Prerequisites

 * Visual Studio 2019 (https://visualstudio.microsoft.com/downloads/) with C# 8.0
 * .NET Framework 4.7.1


### Running the app

 * Compile the project to create geteduroam.exe (in WpfApp\bin\Release).
 * This executable can be run independently from the rest of the solution, so you can move it to any desired directory.


## Signing

When you have a hardware token, you can sign the application

 * Download and install the [Microsoft Windows SDK 10](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk)
   * You need to install the **Windows SDK Signing Tools for Desktop Apps** feature, you can disable all the other features
 * Go to **C:\Program Files (x86)\Windows Kits\10\bin** and find the latest **10.x** version, at the time of writing it's **10.0.18362.0**
 * Inside the **10.x** folder, there's a folder **x64** and in there is a file **signtool.exe**

(actually, just go to **C:\Program Files(x86)** and search for **signtool.exe** and use the one that's in an **x64** directory)

Sign by running

	"C:\Program Files (x86)\Windows Kits\10\bin\10.x\x64\signtool.exe" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a C:\Users\jornane\Source\Repos\EduroamApp\EduroamApp\obj\Release\geteduroam.exe

You can drag files to the command window to write their whole paths.

The timestamp is needed so that the signature remains valid even when the code signing certificate expires

For more information, [check this guide from Digicert](https://www.digicert.com/kb/code-signing/signcode-signtool-command-line.htm)

## Dependencies

These dependencies are managed with `NuGet`, native to Visual Studio. The exception being ManagedNativeWifi which has been cloned and patched.

 * ManagedNativeWifi by emoacht (https://github.com/emoacht/ManagedNativeWifi) ~ [LICENSE](Licenses/ManagedNativeWifi_LICENSE.md)
 * Newtonsoft.Json by JamesNK (https://github.com/JamesNK/Newtonsoft.Json) ~ [LICENSE](Licenses/Newtonsoft.Json_LICENSE.md)
 * Fody (https://github.com/Fody/Fody) ~ [LICENSE](Licenses/Fody_LICENSE.md)
 * Costura (https://github.com/Fody/Costura) ~ [LICENSE](Licenses/Costura_LICENSE.md)
 * DuoVia.FuzzyString by Tyler Jensen (https://www.nuget.org/packages/System.Collections.Immutable/) ~ Apache-2.0
 * System.Collections.Immutable (https://www.nuget.org/packages/System.Collections.Immutable/) ~ MIT LICENSE
 * SingleInstanceApp (https://github.com/taylorjonl/SingleInstanceApp)

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE.md](LICENSE.md) file for details.
