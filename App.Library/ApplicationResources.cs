using System;
using System.Linq;
using System.Reflection;

namespace App.Library
{
    internal static class ApplicationResources
    {
        /// <summary>
        /// Get a resource string from the App specific resources.resx.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        ///     Resource string if Resources are found in the App assembly and if the Resources contain a resource for the given key. 
        ///     Returns null otherwise.
        /// </returns>
        internal static string? GetString(string key)
        {
            var appResources = Assembly.GetEntryAssembly()!.GetManifestResourceNames();
            var appLocalizationResourcesName = appResources.FirstOrDefault(resource => resource.EndsWith("Resources.resources"));

            if (appLocalizationResourcesName != null)
            {
                var resourceManager = new global::System.Resources.ResourceManager(appLocalizationResourcesName.Replace(".Resources.", "."), Assembly.GetEntryAssembly()!);
                return resourceManager.GetString(key);
            }

            throw new System.Resources.MissingManifestResourceException($"Cannot find Resources in {Assembly.GetEntryAssembly()!.GetName().Name}");
        }
    }
}
