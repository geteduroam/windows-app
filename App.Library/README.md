# App library
App library project contains all Xaml related resources for use by the apps (Eduroam.App and Govroam.App)

## Structure
* MainWindow.xaml: [Window xaml](https://learn.microsoft.com/en-us/dotnet/api/system.windows.window?view=windowsdesktop-7.0&f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.Windows.Window)%3Bk(VS.XamlEditor)%26rd%3Dtrue), contains application resources, like the Styling/Styling.xaml.
* Templates/*: Page xaml templates. Each template contains a reference to a ViewModel.
  ``` 
  <DataTemplate DataType="{x:Type viewModels:...ViewModel}"> 
  ```
* ViewModels/*: View models with properties and (page) logic.
* Styling/Styling.xml: xaml containing generic styling resources, like a base background color.
* Binding: extensions for binding Wpf control properties.
* Command: extensions for binding Wpf control events.
* Converters: extensions for converting values from/to wpf controls.
* Images: contains static images and a utility to load dynamic images.
* Connections: support for different roaming connection types.
* Utility: utility classes.

## Localization
App library uses localization resources from EduRoam.Localization (shared resources) and Eduroam.App/Govroam.App (app specific resources)