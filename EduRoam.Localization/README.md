# EduRoam.Localization

`EduRoam.Localization` contains the multi-language ResX string resources and localization infrastructure used across all geteduroam desktop and CLI applications.

- **Target Frameworks**: `net8.0-windows10.0.19041.0;netstandard2.0`
- **Supported Cultures**: 20+ languages including English (default), Arabic (`ar`), Bulgarian (`bg_BG`), Catalan (`ca_ES`), Czech (`cs_CZ`), Welsh (`cy`), German (`de`), Spanish (`es_ES`), Estonian (`et_EE`), French (`fr_FR`), Italian (`it_IT`), Dutch (`nl`), Polish (`pl_PL`), Portuguese (`pt_PT`), Romanian (`ro_RO`), Slovenian (`sl_SI`), and Ukrainian (`uk_UA`).

---

## Usage in Applications

### 1. Binding in XAML (`App.Library` & WPF Apps)

Import the localization namespace in the root XAML element:

```xml
xmlns:lang="clr-namespace:EduRoam.Localization;assembly=EduRoam.Localization"
```

Bind localized string resources using the `x:Static` markup extension:

```xml
<TextBlock Text="{x:Static lang:Resources.SelectInstitution}" />
<Button Content="{x:Static lang:Resources.Connect}" />
```

### 2. Accessing in C# Code

Access typed properties directly via `EduRoam.Localization.Resources`:

```csharp
using EduRoam.Localization;

string message = Resources.ErrorNoInternet;
```

### 3. Setting Current UI Culture

During application startup (`App.xaml.cs` or `Program.cs`), set the resources culture:

```csharp
Resources.Culture = System.Globalization.CultureInfo.CurrentUICulture;
```

---

## Adding or Updating Translations

1. **Locate or Create Culture ResX**:
   - The default English resources are in `Resources.resx`.
   - Culture-specific files follow standard .NET naming conventions: `Resources.<culture-code>.resx` (e.g., `Resources.de.resx`, `Resources.fr_FR.resx`).
2. **Translate Resource Entries**:
   - Ensure the resource keys match the master `Resources.resx` file.
3. **Single-File Publishing Integration**:
   - In .NET 8 SDK, satellite resource assemblies (`<culture>/EduRoam.Localization.resources.dll`) are automatically compiled and bundled directly into the single-file self-contained executable when running `dotnet publish -p:PublishSingleFile=true`.
4. **Rebuild Requirement**:
   - When modifying `.resx` files, perform a clean **Rebuild** (`dotnet build --no-incremental`) to ensure satellite assemblies and `Resources.Designer.cs` code generators are fully refreshed.
