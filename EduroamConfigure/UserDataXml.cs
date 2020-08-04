using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EduroamConfigure
{
	/// <summary>
	/// User data XML generator.
	/// </summary>
	/// <remarks>
	/// Documentation:
	///
	/// https://docs.microsoft.com/en-us/windows/win32/eaphost/eaphostusercredentialsschema-schema
	/// https://docs.microsoft.com/en-us/windows/win32/eaphost/user-profiles
	/// https://github.com/rozmansi/WLANSetEAPUserData/tree/master/Examples
	/// C:\Windows\schemas\EAPMethods
	/// C:\Windows\schemas\EAPHost
	/// </remarks>
	class UserDataXml
	{
		// Namespaces:

		static readonly XNamespace nsEHUC = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
		static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		static readonly XNamespace nsBEMUC = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";
		static readonly XNamespace nsEUP = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
		static readonly XNamespace nsXSI = "http://www.w3.org/2001/XMLSchema-instance";
		static readonly XNamespace nsBEUP = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";

		// PEAP / MSCHAPv2 specific
		static readonly XNamespace nsMPUP = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
		static readonly XNamespace nsMCUP = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";

		// TTLS specific
		static readonly XNamespace nsTTLS = "http://www.microsoft.com/provisioning/EapTtlsUserPropertiesV1";

		// TLS specific
		static readonly XNamespace nsTLS = "http://www.microsoft.com/provisioning/EapTlsUserPropertiesV1";

		/// <summary>
		/// Generates user data xml.
		/// </summary>
		/// <param name="authMethod">authMethod</param>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		/// <returns>Complete user data xml as string.</returns>
		public static string CreateUserDataXml(
			EapConfig.AuthenticationMethod authMethod,
			string username,
			string password)
		{
			_ = authMethod ?? throw new ArgumentNullException(nameof(authMethod));
			using var userCert = authMethod.ClientCertificateAsX509Certificate2();

			XElement newUserData =
				new XElement(nsEHUC + "EapHostUserCredentials",
					new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
					new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
					new XElement(nsEHUC + "EapMethod",
						new XElement(nsEC + "Type", (int)authMethod.EapType),
						new XElement(nsEC + "AuthorId", authMethod.EapType == EapType.TTLS ? 311 : 0)
						//new XElement(nsEC + "AuthorId", "67532") // geant link
					),
					new XElement(nsEHUC + "Credentials",
						new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
						new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
						new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
						new XAttribute(XNamespace.Xmlns + "MsPeap", nsMPUP),
						new XAttribute(XNamespace.Xmlns + "MsChapV2", nsMCUP),
						new XAttribute(XNamespace.Xmlns + "eapTtls", nsTTLS),
						EapUserData(
							username ?? authMethod.ClientUserName,
							password ?? authMethod.ClientPassword,
							outerIdentity:
								!string.IsNullOrEmpty(authMethod.ClientOuterIdentity)
									? authMethod.ClientOuterIdentity
									: username,
							authMethod.EapType,
							authMethod.InnerAuthType,
							userCert?.Thumbprint
						)
					)
				);

			// returns xml as string if not null
			return newUserData != null ? newUserData.ToString() : "";
		}
		private static XElement EapUserData(
			string innerIdentity,
			string password,
			string outerIdentity,
			EapType eapType,
			InnerAuthType innerAuthType,
			string userCertFingerprint = null)
		{
			return (eapType, innerAuthType) switch
			{
				(EapType.MSCHAPv2, InnerAuthType.None) =>

					new XElement(nsBEUP + "Eap",
						new XElement(nsBEUP + "Type", (int)EapType.MSCHAPv2),
						new XElement(nsMCUP + "EapType",
							new XElement(nsMCUP + "Username", innerIdentity),
							new XElement(nsMCUP + "Password", password),
							new XElement(nsMCUP + "LogonDomain") // TODO: what is this?
						)
					),

				(EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) =>

					new XElement(nsBEUP + "Eap",
						new XElement(nsBEUP + "Type", (int)EapType.PEAP),
						new XElement(nsMPUP + "EapType",
							new XElement(nsMPUP + "RoutingIdentity", outerIdentity),
							EapUserData(
								innerIdentity,
								password,
								outerIdentity,
								EapType.MSCHAPv2,
								InnerAuthType.None
							)
						)
					),

				(EapType.TLS, InnerAuthType.None) =>

					new XElement(nsBEUP + "Eap",
						new XElement(nsBEUP + "Type", (int)EapType.TLS),
						new XElement(nsTLS + "EapType",
							new XElement(nsTLS + "Username", outerIdentity), // TODO: test if this gets used
							new XElement(nsTLS + "UserCert", // xs:hexBinary
								// format fingerprint:
								Regex.Replace(Regex.Replace(userCertFingerprint, " ", ""), ".{2}", "$0 ")
									.ToUpperInvariant().Trim()
							)
						)
					),

				var x when
				x == (EapType.TTLS, InnerAuthType.PAP) ||
				x == (EapType.TTLS, InnerAuthType.MSCHAP) || // v1 is not tested
				x == (EapType.TTLS, InnerAuthType.MSCHAPv2) =>

					new XElement(nsTTLS + "EapTtls", // schema says lower camelcase, but only upper camelcase works
						new XElement(nsTTLS + "Username", innerIdentity), // outerIdentity is configured in ProfileXml
						new XElement(nsTTLS + "Password", password)
					),


				(EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => // TODO: matches schema, but produces an error

					new XElement(nsTTLS + "EapTtls",
						//new XElement(nsTTLS + "Username", uname),
						//new XElement(nsTTLS + "Password", pword),
						EapUserData(
							innerIdentity,
							password,
							outerIdentity,
							EapType.MSCHAPv2,
							InnerAuthType.None
						)
					),

				(EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => // TODO: matches schema, but produces an error

					new XElement(nsTTLS + "EapTtls",
						//new XElement(nsTTLS + "Username", uname),
						//new XElement(nsTTLS + "Password", pword),
						EapUserData(
							innerIdentity,
							password,
							outerIdentity,
							EapType.MSCHAPv2,
							InnerAuthType.None
						)
					),

				// not supported
				_ => throw new EduroamAppUserError("unsupported auth method"),
			};
		}

		/// <summary>
		/// If this returns False, then ther innerIdentity/username and the passord fields may be set to null.
		/// </summary>
		/// <param name="authMethod">the authentificsation method to check</param>
		/// <returns></returns>
		public static bool NeedsLoginCredentials(EapConfig.AuthenticationMethod authMethod)
		{
			return authMethod.EapType != EapType.TLS;
		}

		public static bool IsSupported(EapConfig.AuthenticationMethod authMethod)
		{
			return IsSupported(authMethod.EapType, authMethod.InnerAuthType);
		}

		public static bool IsSupported(EapType eapType, InnerAuthType innerAuthType)
		{
			//bool at_least_win10 = System.Environment.OSVersion.Version.Major >= 10; // TODO: make this work, requires some application manifest
			var at_least_win10 = true;
			return (eapType, innerAuthType) switch
			{
				(EapType.TLS, _) => true,
				(EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) => true,
				(EapType.TTLS, InnerAuthType.PAP) => true,
				(EapType.TTLS, InnerAuthType.MSCHAP) => true, // not tested
				(EapType.TTLS, InnerAuthType.MSCHAPv2) => true,
				//(EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => at_least_win10, // TODO: xml matches the schema, but win32 throws an error.
				//(EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => at_least_win10, // TODO: xml matches the schema, but win32 throws an error.
				_ => false,
			};
		}
	}
}
