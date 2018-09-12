# eduroam app for Windows

This application helps set up eduroam on end-users' computers by automatically fetching and installing the required certificates.

## Getting started

### Prerequisites

 * Visual Studio 2017 (https://visualstudio.microsoft.com/downloads/)
 * .NET Framework 4.7.1
 
### Running the app

 * Compile the project to create EduroamApp.exe (in EduroamApp\EduroamApp\bin\Debug). 
	* This executable can be run independently from the rest of the solution, so you can move it to any desired directory.

## Dependencies

 * ManagedNativeWifi by emoacht (https://github.com/emoacht/ManagedNativeWifi)
	* [LICENSE](Licenses/ManagedNativeWifi_LICENSE.md)
 * Newtonsoft.Json by JamesNK (https://github.com/JamesNK/Newtonsoft.Json)
	* [LICENSE](Licenses/Newtonsoft.Json_LICENSE.md)
 * Fody (https://github.com/Fody/Fody)
	* [LICENSE](Licenses/Fody_LICENSE.md)
* Costura (https://github.com/Fody/Costura)
	* [LICENSE](Licenses/Costura_LICENSE.md)

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE.md](LICENSE.md) file for details.