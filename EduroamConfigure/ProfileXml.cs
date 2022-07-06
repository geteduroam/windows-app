using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EduroamConfigure
{
	/// <summary>
	/// Wireless profile XML generator.
	/// </summary>
	/// <remarks>
	/// Documentation of the XML format:
	///
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-elements
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/onexschema-elements
	/// https://docs.microsoft.com/en-us/windows/win32/eaphost/eaptlsconnectionpropertiesv1schema-servervalidationparameters-complextype
	/// https://docs.microsoft.com/en-us/powershell/module/vpnclient/new-eapconfiguration?view=win10-ps
	/// https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-gpwl/7fda6c4b-0347-466c-926f-0e7e45a0aa7a
	/// C:\Windows\schemas\EAPMethods
	/// C:\Windows\schemas\EAPHost
	/// </remarks>
	class ProfileXml
	{
		// Namespaces:

		// WLANProfile
		static readonly XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
		static readonly XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
		static readonly XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
		static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		static readonly XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";

		//static readonly XNamespace nsHSP = "http://www.microsoft.com/networking/WLAN/HotspotProfile/v1";

		// TLS specific
		static readonly XNamespace nsETCPv1 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";
		static readonly XNamespace nsETCPv2 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV2";
		static readonly XNamespace nsETCPv3 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV3";

		// MSCHAPv2 specific
		static readonly XNamespace nsMPCPv1 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";
		static readonly XNamespace nsMPCPv2 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2";
		static readonly XNamespace nsMPCPv3 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3";
		static readonly XNamespace nsMCCP = "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1";

		// TTLS specific
		static readonly XNamespace nsTTLS = "http://www.microsoft.com/provisioning/EapTtlsConnectionPropertiesV1";

		private static readonly string[] PREFERRED_SSIDS = new string[] { "eduroam", "govroam" };

		public static ValueTuple<string, string> CreateSSIDProfileXml(EapConfig.AuthenticationMethod authMethod, string ssid)
			=> CreateProfileXml(authMethod, withSSID: ssid);
		public static ValueTuple<string, string> CreateHS20ProfileXml(EapConfig.AuthenticationMethod authMethod)
			=> CreateProfileXml(authMethod, withHS20: true);

		/// <summary>
		/// Generates wireless profile xml. Content depends on the EAP type.
		/// </summary>
		/// <param name="authMethod">authMethod</param>
		/// <param name="withSSID">TODO</param>
		/// <param name="withHS20">If to install as hotspot 2.0 profile or not (separate profile from normal eap)</param>
		/// <returns>A tuple containing the profile name and the WLANProfile XML data</returns>
		private static ValueTuple<string, string> CreateProfileXml(
			EapConfig.AuthenticationMethod authMethod,
			string withSSID = null,
			bool withHS20 = false,
			bool hiddenNetwork = false)
		{
			if (withHS20 && withSSID != null)
				throw new ArgumentException("Cannot configure with both SSID and HS20"); // we can, but the result is confusing
			if (withSSID != null && !authMethod.IsSSIDSupported)
				throw new ArgumentException("Cannot configure " + nameof(authMethod) + " with SSID because it doesn't support SSID configuration");
			if (withHS20 && !authMethod.IsHS20Supported)
				throw new ArgumentException("Cannot configure " + nameof(authMethod) + " with Hotspot 2.0 because it doesn't support Hotspot 2.0 configuration");
			if (withSSID != null && !authMethod.SSIDs.Any((ssid) => withSSID == ssid))
				throw new ArgumentException("The ssid is not used by the authentication method");

			if (authMethod.ServerNames.Count == 0 || authMethod.ServerCertificateAuthorities.Count == 0)
			{
				throw new ArgumentException("The authentication method must have server certificate validation through server name and allowed CA");
			}

			// Decide the profile name, which is the unique identifier for this profile
			string profileName = null;
			if (withHS20 && string.IsNullOrWhiteSpace(profileName))
				profileName = authMethod.EapConfig.InstitutionInfo.DisplayName;
			if (withSSID != null && string.IsNullOrWhiteSpace(profileName))
				profileName = withSSID;
			if (string.IsNullOrWhiteSpace(profileName)) foreach (string preferredSSID in PREFERRED_SSIDS)
				{
					if (authMethod.SSIDs.Contains(preferredSSID))
					{
						profileName = preferredSSID;
						break;
					}
				}
			if (string.IsNullOrWhiteSpace(profileName) && authMethod.SSIDs.Any())
			{
				profileName = authMethod.SSIDs.First();
			}
			if (string.IsNullOrWhiteSpace(profileName) && authMethod.ConsortiumOIDs.Any())
			{
				profileName = authMethod.ConsortiumOIDs.First();
			}
			if (withHS20 && authMethod.SSIDs.Contains(profileName))
			{
				// since profileName is the unique identifier of the profile. avoid collisions with the profiles per ssid
				profileName += " via Passpoint"; // GEANT convention as fallback
			}

			// Construct XML document
			XElement ssidConfigElement;
			XElement hs2Element, roamingConsortiumElement;
			XElement newProfile =
				new XElement(nsWLAN + "WLANProfile",
					new XElement(nsWLAN + "name", profileName),
					ssidConfigElement =
					new XElement(nsWLAN + "SSIDConfig"),
					hs2Element =
					new XElement(nsWLAN + "Hotspot2",
						new XElement(nsWLAN + "DomainName", authMethod.EapConfig.InstitutionInfo.InstId),
						//new XElement(nsWLAN + "NAIRealm", ), // A list of Network Access Identifier (NAI) Realm identifiers. Entries in this list are usually of the form user@domain.
						// new XElement(nsWLAN + "Network3GPP", ), // A list of Public Land Mobile Network (PLMN) IDs.
						roamingConsortiumElement =
						new XElement(nsWLAN + "RoamingConsortium") // A list of Organizationally Unique Identifiers (OUI) assigned by IEEE.
					),
					new XElement(nsWLAN + "connectionType", "ESS"),
					new XElement(nsWLAN + "connectionMode", "auto"),
					new XElement(nsWLAN + "autoSwitch", "false"),
					new XElement(nsWLAN + "MSM",
						new XElement(nsWLAN + "security",
							new XElement(nsWLAN + "authEncryption",
								new XElement(nsWLAN + "authentication", "WPA2"),
								new XElement(nsWLAN + "encryption", "AES"), // CredentialApplicability.MinRsnProto is forced to not be TKIP
								new XElement(nsWLAN + "useOneX", "true")
							),
							new XElement(nsWLAN + "PMKCacheMode", "enabled"),
							new XElement(nsWLAN + "PMKCacheTTL", "720"),
							new XElement(nsWLAN + "PMKCacheSize", "128"),
							new XElement(nsWLAN + "preAuthMode", "disabled"),
							new XElement(nsOneX + "OneX",
								new XElement(nsOneX + "authMode", "user"),
								new XElement(nsOneX + "EAPConfig",
									CreateEapConfiguration(
										eapType: authMethod.EapType,
										innerAuthType: authMethod.InnerAuthType,
										outerIdentity: authMethod.ClientOuterIdentity,
										serverNames: authMethod.ServerNames,
										caThumbprints: authMethod.CertificateAuthoritiesAsX509Certificate2()
											.Where(cert => cert.Subject == cert.Issuer)
											.Select(cert => cert.Thumbprint).ToList()
									)
								)
							)
						)
					)
				);


			// Add all the supported SSIDs, if we have none, assume we're doing HS20 if we got this far and nobody stopped us
			var ssids = authMethod.SSIDs.Any() ? authMethod.SSIDs : new List<string> { "#Passpoint" };
			ssids.ForEach(ssid => // This element supports up to 25 SSIDs in the v1 namespace and up to additional 10000 SSIDs in the v2 namespace.
				ssidConfigElement.Add(
					new XElement(nsWLAN + "SSID",
						//new XElement(nsWLAN + "hex", ssidHex),
						new XElement(nsWLAN + "name", ssid)
					)
				));
			ssidConfigElement.Add(
				new XElement(nsWLAN + "nonBroadcast", hiddenNetwork ? "true" : "false")
			);

			// Populate the Hs2 field
			authMethod.ConsortiumOIDs.ForEach(oui =>
				roamingConsortiumElement.Add(
					new XElement(nsWLAN + "OUI", oui)
				));
			// ... or remove it if it shouldn't be there
			if (!withHS20) hs2Element.Remove();

			// return xml as string
			return (profileName, newProfile.ToString());
		}

		private static XElement CreateEapConfiguration(
			EapType eapType,
			InnerAuthType innerAuthType,
			string outerIdentity,
			List<string> serverNames,
			List<string> caThumbprints)
		{
			// Typically, this should be on ALWAYS, BUT:
			// If the outer type is TTLS, we recursively get back here again,
			// and we cannot do inner validation.
			bool enableServerValidation = serverNames.Any() || caThumbprints.Any();

			// creates the root xml strucure, with references to some of its descendants
			XElement configElement;
			XElement serverValidationElement;
			XElement caHashListElement = null; // eapType == eapType.TLS only
			XElement eapConfiguration =
				new XElement(nsEHC + "EapHostConfig",
					new XElement(nsEHC + "EapMethod",
						new XElement(nsEC + "Type", (int)eapType),
						new XElement(nsEC + "VendorId", 0),
						new XElement(nsEC + "VendorType", 0),
						new XElement(nsEC + "AuthorId", eapType == EapType.TTLS ? 311 : 0) // no geant link
					),
					configElement =
					new XElement(nsEHC + "Config")
				);

			// namespace element local names dependant on EAP type
			XNamespace nsEapType;
			string thumbprintNodeName;

			if ((eapType, innerAuthType) == (EapType.TLS, InnerAuthType.None))
			{
				// sets namespace and name of thumbprint node
				nsEapType = nsETCPv1;
				thumbprintNodeName = "TrustedRootCA";

				// adds TLS specific xml elements
				configElement.Add(
					new XElement(nsBECP + "Eap",
						new XElement(nsBECP + "Type", (int)eapType), // TLS
						new XElement(nsETCPv1 + "EapType",
							new XElement(nsETCPv1 + "CredentialsSource",
								new XElement(nsETCPv1 + "CertificateStore",
									new XElement(nsETCPv1 + "SimpleCertSelection", "true")
								)
							),
							serverValidationElement =
							new XElement(nsETCPv1 + "ServerValidation",
								new XElement(nsETCPv1 + "DisableUserPromptForServerValidation", enableServerValidation ? "true" : "false"),
								new XElement(nsETCPv1 + "ServerNames", string.Join(";", serverNames))
							),
							new XElement(nsETCPv1 + "DifferentUsername", "false"),
							new XElement(nsETCPv2 + "PerformServerValidation", "true"),
							new XElement(nsETCPv2 + "AcceptServerName", "false"),
							new XElement(nsETCPv2 + "TLSExtensions",
								new XElement(nsETCPv3 + "FilteringInfo",
									caHashListElement =
									new XElement(nsETCPv3 + "CAHashList", new XAttribute("Enabled", "true"))
								)
							)
						)
					)
				);
			}
			else if ((eapType, innerAuthType) == (EapType.MSCHAPv2, InnerAuthType.None))
			{
				// MSCHAPv2 as outer EAP type should only be used in a TTLS tunnel
				// It does not support server validation
				if (enableServerValidation)
					throw new EduroamAppUserException("not supported",
						"MSCHAPv2 as outer EAP does not support server validation");
				nsEapType = null;
				thumbprintNodeName = null;
				serverValidationElement = null;

				// adds MSCHAPv2 specific elements (inner eap)
				configElement.Add(
					new XElement(nsBECP + "Eap", // MSCHAPv2
						new XElement(nsBECP + "Type", (int)eapType),
						new XElement(nsMCCP + "EapType",
							new XElement(nsMCCP + "UseWinLogonCredentials", "false")
						)
					)
				);
			}
			else if ((eapType, innerAuthType) == (EapType.PEAP, InnerAuthType.EAP_MSCHAPv2))
			{
				// sets namespace and name of thumbprint node
				nsEapType = nsMPCPv1;
				thumbprintNodeName = "TrustedRootCA";

				// Windows wants to add the realm itself, we must only set the local part
				// This appears to be the case for PEAP-EAP-MSCHAPv2
				string anonymousUserName = outerIdentity.Contains("@")
					? outerIdentity.Substring(0, outerIdentity.IndexOf("@"))
					: outerIdentity
					;

				// adds MSCHAPv2 specific elements (inner eap)
				configElement.Add(
					new XElement(nsBECP + "Eap", // PEAP
						new XElement(nsBECP + "Type", (int)eapType),
						new XElement(nsMPCPv1 + "EapType",
							serverValidationElement =
							new XElement(nsMPCPv1 + "ServerValidation",
								new XElement(nsMPCPv1 + "DisableUserPromptForServerValidation", enableServerValidation ? "true" : "false"),
								new XElement(nsMPCPv1 + "ServerNames", string.Join(";", serverNames))
							),
							new XElement(nsMPCPv1 + "FastReconnect", "true"),
							new XElement(nsMPCPv1 + "InnerEapOptional", "false"),
							new XElement(nsBECP + "Eap", // MSCHAPv2
								new XElement(nsBECP + "Type", (int)innerAuthType),
								new XElement(nsMCCP + "EapType",
									new XElement(nsMCCP + "UseWinLogonCredentials", "false")
								)
							),
							new XElement(nsMPCPv1 + "EnableQuarantineChecks", "false"),
							new XElement(nsMPCPv1 + "RequireCryptoBinding", "false"),
							new XElement(nsMPCPv1 + "PeapExtensions",
								new XElement(nsMPCPv2 + "PerformServerValidation", "true"),
								new XElement(nsMPCPv2 + "AcceptServerName", "true"),
								String.IsNullOrWhiteSpace(anonymousUserName)
									? new XElement(nsMPCPv2 + "IdentityPrivacy",
										new XElement(nsMPCPv2 + "EnableIdentityPrivacy", "false")
									)
									: new XElement(nsMPCPv2 + "IdentityPrivacy",
										new XElement(nsMPCPv2 + "EnableIdentityPrivacy", "true"),
										new XElement(nsMPCPv2 + "AnonymousUserName", anonymousUserName)
									)
								,

								// Here, the ordering is important.  If PeapExtensionsV2 is not last, SOME devices will throw an error.
								// The profile throws a W32Exception ErrorCode 1206 (corrupt profile) ReasonCode 1 (unenumerated at the time of writing)
								// The reason is probably that the V1 schema specifies a specific ordering of the elements,
								// which up until to now we never saw was enforced.
								// https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-gpwl/0673b15a-492f-4e7d-b15b-61a329293e80

								// A confirmed problematic device ran on:
								// Windows 10 (OS Build 17134.1550)
								// https://support.microsoft.com/en-us/topic/june-9-2020-kb4561621-os-build-17134-1550-2b74db42-3293-808c-199e-eb4130982afe
								// Intel(R) Dual Band Wireless-AC 7265
								// Driver Version 19.51.24.3 (8/26/2019)
								new XElement(nsMPCPv2 + "PeapExtensionsV2",
									new XElement(nsMPCPv3 + "AllowPromptingWhenServerCANotFound", "true")
								)
							)
						)
					)
				);
			}
			else if (eapType == EapType.TTLS)
			{
				// sets namespace and name of thumbprint node
				nsEapType = nsTTLS;
				thumbprintNodeName = "TrustedRootCAHash";

				configElement.Add(
					new XElement(nsTTLS + "EapTtls",
						serverValidationElement =
						new XElement(nsTTLS + "ServerValidation",
							new XElement(nsTTLS + "ServerNames", string.Join(";", serverNames)),
							new XElement(nsTTLS + "DisablePrompt", enableServerValidation ? "true" : "false")
						),
						new XElement(nsTTLS + "Phase2Authentication",
							innerAuthType switch
							{
								InnerAuthType.PAP =>
									new XElement(nsTTLS + "PAPAuthentication"),
								//InnerAuthType.CHAP => // not defined by EapConfig
								//    new XElement(nsTTLS + "CHAPAuthentication"),
								InnerAuthType.MSCHAP =>
									new XElement(nsTTLS + "MSCHAPAuthentication"),
								InnerAuthType.MSCHAPv2 =>
									new XElement(nsTTLS + "MSCHAPv2Authentication",
										new XElement(nsTTLS + "UseWinlogonCredentials", "false")
									),
								/*
								// Probably not in use anywhere
								InnerAuthType.EAP_PEAP_MSCHAPv2 =>
									CreateEapConfiguration(
										eapType: EapType.PEAP,
										innerAuthType: InnerAuthType.EAP_MSCHAPv2,
										outerIdentity: outerIdentity,
										// Strip server names and thumbprints from inner EAP, only need in outer
										serverNames: new List<string>(),
										caThumbprints: new List<string>()
									),
								*/
								InnerAuthType.EAP_MSCHAPv2 => // Sometimes just called TTLS-EAP
									CreateEapConfiguration(
										eapType:EapType.MSCHAPv2,
										innerAuthType: InnerAuthType.None,
										outerIdentity: null, // Not relevant for inner auth
										// Strip server names and thumbprints from inner EAP, only need in outer
										serverNames: new List<string>(),
										caThumbprints: new List<string>()
									),
								_ =>
									throw new EduroamAppUserException("unsupported auth method"),
							}
						),
						String.IsNullOrWhiteSpace(outerIdentity)
							? new XElement(nsTTLS + "Phase1Identity",
								new XElement(nsTTLS + "IdentityPrivacy", "false")
							)
							: new XElement(nsTTLS + "Phase1Identity",
								new XElement(nsTTLS + "IdentityPrivacy", "true"),
								new XElement(nsTTLS + "AnonymousIdentity", outerIdentity)
							)
					)
				);
			}
			else
			{
				throw new EduroamAppUserException("unsupported auth method");
			}

			// Server validation
			if (caThumbprints.Any())
			{
				// Format the CA thumbprints into xs:element type="hexBinary"
				List<string> formattedThumbprints = caThumbprints
					.Select(thumb => Regex.Replace(thumb, " ", ""))
					.Select(thumb => Regex.Replace(thumb, ".{2}", "$0 "))
					.Select(thumb => thumb.ToUpperInvariant())
					.Select(thumb => thumb.Trim())
					.ToList();

				// Write the CA thumbprints to their proper places in the XML:

				if (serverValidationElement != null) // Not on bare MSCHAPv2
					formattedThumbprints.ForEach(thumb =>
						serverValidationElement.Add(new XElement(nsEapType + thumbprintNodeName, thumb)));

				if (caHashListElement != null) // TLS only
					formattedThumbprints.ForEach(thumb =>
						caHashListElement.Add(new XElement(nsETCPv3 + "IssuerHash", thumb)));
			}

			return eapConfiguration;
		}

		/// <summary>
		/// Use this to determine if the authMethod can be installed as a WLanProfile
		/// </summary>
		/// <param name="authMethod"></param>
		/// <returns></returns>
		public static bool IsSupported(EapConfig.AuthenticationMethod authMethod)
		{
			// check if it has a supported
			if (authMethod.EapConfig.CredentialApplicabilities
				.Where(cred => cred.NetworkType == IEEE802x.IEEE80211)
				.Where(cred => cred.MinRsnProto != "TKIP") // too insecure
				.Any())
			{
				return IsSupported(authMethod.EapType, authMethod.InnerAuthType);
			}
			return false;
		}

		private static bool IsSupported(EapType eapType, InnerAuthType innerAuthType)
		{
			//bool at_least_win10 = System.Environment.OSVersion.Version.Major >= 10; // TODO: make this work, requires some application manifest
			var at_least_win10 = true;
			return (eapType, innerAuthType) switch
			{
				(EapType.MSCHAPv2, InnerAuthType.None) => true,
				(EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) => true,
				(EapType.TLS, InnerAuthType.None) => true,
				(EapType.TTLS, InnerAuthType.PAP) => true,
				(EapType.TTLS, InnerAuthType.MSCHAP) => true,
				(EapType.TTLS, InnerAuthType.MSCHAPv2) => true,
				(EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => at_least_win10, // Sometimes just called TTLS-EAP
				//(EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => at_least_win10, // theoretically supported, but we don't know any server
				_ => false,
			};
		}
	}

}
