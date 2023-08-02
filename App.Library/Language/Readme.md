# Language
Titles, labels, error messages etc. are stored in the Resources .resx files (one per language).

Resources are bound in xaml by adding the following namespace in the top (for example <Window> or <ResourceDictionary>) element:
```xmlns:lang = "clr-namespace:App.Library.Language"```

Bind a resource using:
```{x:Static lang:Resources.<resource>}```

For example:
````
Text="{x:Static lang:Resources.Loading}"
````

## Adding a new language
1. Copy an existing .resx file. (e.g. Resources.nl-nl.resx)
1. Update the culture in the new filename, and remove the " - Copy" part.
1. Open the new .resx and update the resources for the new language.
