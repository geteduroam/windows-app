﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Data encryption type to be used to connect to wireless LAN
	/// </summary>
	/// <remarks>
	/// Equivalent to encryption element in profile XML:
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-encryption-authencryption-element
	/// </remarks>
	public enum EncryptionType
	{
		/// <summary>
		/// None (valid value)
		/// </summary>
		None = 0,

		/// <summary>
		/// WEP encryption for WEP
		/// </summary>
		WEP,

		/// <summary>
		/// TKIP encryption for WPA/WPA2
		/// </summary>
		TKIP,

		/// <summary>
		/// AES (CCMP) encryption for WPA/WPA2
		/// </summary>
		AES
	}

	internal static class EncryptionTypeConverter
	{
		public static bool TryParse(string source, out EncryptionType encryption)
		{
			switch (source)
			{
				case "WEP":
					encryption = EncryptionType.WEP;
					return true;
				case "TKIP":
					encryption = EncryptionType.TKIP;
					return true;
				case "AES":
					encryption = EncryptionType.AES;
					return true;
				case "none":
					encryption = EncryptionType.None;
					return true;
			}
			encryption = default;
			return true;
		}
	}
}