using Microsoft.Win32;

namespace EduroamConfigure
{
	class PersistingStore
	{
		/// <summary>
		/// Username of
		/// </summary>
		public static string Username
		{
			get => getValue("username");
			set => setValue("username", value);
		}

		// TODO: persist the static state of EduroamNetworks using this class

		// TODO: use json instead?

		private const string ns = "HKEY_CURRENT_USER\\RegistrySetValueExample"; // Namespace
		private static string getValue(string key)
		{
			return (string)Registry.GetValue(ns, key, null);
		}
		private static void setValue(string key, string value)
		{
			Registry.SetValue(ns, key, value);
		}
	}
}
