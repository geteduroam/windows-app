# Language
Titles, labels, error messages etc. are stored in the Resources .resx files (one per culture).

Resources are bound in xaml by adding the following namespace in the top (for example <Window> or <ResourceDictionary>) element:
`xmlns:lang = "clr-namespace:EduRoam.Localization;assembly=EduRoam.Localization"`

Then, bind a resource using:
`{x:Static lang:Resources.<resource>}`

For example:
```
Text="{x:Static lang:Resources.Loading}"
```

## UI Culture
The resources culture is set to the Current UI Culture in the program entry class (Program.cs for console app en App.xaml.cs for WPF app)

Setting resource culture:
``` csharp
Resources.Culture = System.Globalization.CultureInfo.CurrentUICulture;
```

## Adding a new culture
1. Copy an existing .resx file. (e.g. Resources.nl-nl.resx)
1. Update the culture in the new filename, and remove the " - Copy" part.
1. Open the new .resx and update the resources for the new language.

# Important
When updating only resources in culture specific resource files (for example Resources.nl-nl.resx) a Rebuild of the apps is necessary to reflect the update(s). Only a Build will not suffice.
