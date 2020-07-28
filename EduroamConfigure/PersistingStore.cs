using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace EduroamConfigure
{

	public class PersistingStore
	{
		/// <summary>
		/// The username to remember from when the user last logged in
		/// </summary>
		public static string Username
		{
			get => GetValue<string>("Username");
			set => SetValue<string>("Username", value);
		}

		/// <summary>
		/// The ID cof the eap-config profile as assigned by discovery.geteduroam.*
		/// </summary>
		public static string ProfileID
		{
			get => GetValue<string>("ProfileID");
			set => SetValue<string>("ProfileID", value);
		}

		// TODO: persist the static state of EduroamNetworks using this class
		/// <summary>
		///
		/// </summary>
		public static ImmutableHashSet<ConfiguredProfile> ConfiguredProfiles
		{
			get => GetValue<ImmutableHashSet<ConfiguredProfile>>("ConfigureProfiles", "[]");
			set => SetValue<ImmutableHashSet<ConfiguredProfile>>("ConfigureProfiles", value);
		}


		public readonly struct ConfiguredProfile
		{
			public Guid InterfaceId { get; }
			public string ProfileName { get; }
			public bool IsHs2 { get; }

			public ConfiguredProfile(Guid interfaceId, string profileName, bool isHs2)
			{
				InterfaceId = interfaceId;
				ProfileName = profileName;
				IsHs2 = isHs2;
			}
		}


		// Inner workings:

		private const string ns = "HKEY_CURRENT_USER\\GetEduroam"; // Namespace in Registry
		private static T GetValue<T>(string key, string defaultJson = "null")
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(
					(string)Registry.GetValue(ns, key, null) ?? defaultJson);
			}
			catch (Newtonsoft.Json.JsonReaderException ex)
			{
				return JsonConvert.DeserializeObject<T>(defaultJson);
			}
		}
		private static void SetValue<T>(string key, T value)
		{
			var serialized = JsonConvert.SerializeObject(value);

			if ((string)Registry.GetValue(ns, key, null) != serialized) // only write when we make a change
			{
				Debug.WriteLine(string.Format("Write to {0}\\{1}: {2}", ns, key, serialized));
				Registry.SetValue(ns, key, serialized);
			}

			return;
		}
	}
}
