using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EduroamApp
{
	class ProfileXml
	{
		// enumerator for EAP types
		public enum EapType
		{
			TLS = 13,
			PEAP_MSCHAPv2 = 25
		}

		// Namespaces
		private static readonly XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
		private static readonly XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
		private static readonly XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
		private static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		private static readonly XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
			// TLS specific
		private static readonly XNamespace nsETCPv1 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";
		private static readonly XNamespace nsETCPv2 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV2";
			// MSCHAPv2 specific
		private static readonly XNamespace nsMPCPv1 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";
		private static readonly XNamespace nsMPCPv2 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2";
		private static readonly XNamespace nsMPCPv3 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3";
		private static readonly XNamespace nsMCCP = "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1";


		/// <summary>
		/// Generates wireless profile xml. Content depends on the EAP type.
		/// </summary>
		/// <param name="ssid">Name of SSID associated with profile.</param>
		/// <param name="eapType">Type of EAP.</param>
		/// <param name="thumbprints">List of CA thumbprints.</param>
		/// <returns>Complete wireless profile xml as string.</returns>
		public static string CreateProfileXml(string ssid, EapType eapType, List<string>  thumbprints)
		{
			uint eap = (uint)eapType; // converts to uint value

			// creates common xml elements
			XElement newProfile =
				new XElement(nsWLAN + "WLANProfile",
					new XElement(nsWLAN + "name", ssid),
					new XElement(nsWLAN + "SSIDConfig",
						new XElement(nsWLAN + "SSID",
							new XElement(nsWLAN + "name", ssid)
						)
					),
					new XElement(nsWLAN + "connectionType", "ESS"),
					new XElement(nsWLAN + "connectionMode", "auto"),
					new XElement(nsWLAN + "MSM",
						new XElement(nsWLAN + "security",
							new XElement(nsWLAN + "authEncryption",
								new XElement(nsWLAN + "authentication", "WPA2"),
								new XElement(nsWLAN + "encryption", "AES"),
								new XElement(nsWLAN + "useOneX", "true")
							),
							new XElement(nsWLAN + "PMKCacheMode", "enabled"),
							new XElement(nsWLAN + "PMKCacheTTL", "720"),
							new XElement(nsWLAN + "PMKCacheSize", "128"),
							new XElement(nsWLAN + "preAuthMode", "disabled"),
							new XElement(nsOneX + "OneX",
								new XElement(nsOneX + "authMode", "user"),
								new XElement(nsOneX + "EAPConfig",
									new XElement(nsEHC + "EapHostConfig",
										new XElement(nsEHC + "EapMethod",
											new XElement(nsEC + "Type", eap),
											new XElement(nsEC + "VendorId", 0),
											new XElement(nsEC + "VendorType", 0),
											new XElement(nsEC + "AuthorId", 0)),
										new XElement(nsEHC + "Config",
											new XElement(nsBECP + "Eap",
												new XElement(nsBECP + "Type", eap),
												new XElement(nsETCPv1 + "EapType",
													new XElement(nsETCPv1 + "CredentialsSource",
														new XElement(nsETCPv1 + "CertificateStore",
															new XElement(nsETCPv1 + "SimpleCertSelection", "true")
														)
													),
													new XElement(nsETCPv1 + "ServerValidation",
														new XElement(nsETCPv1 + "DisableUserPromptForServerValidation", "false")
													),
													new XElement(nsETCPv1 + "DifferentUsername", "false"),
													new XElement(nsETCPv2 + "PerformServerValidation", "true"),
													new XElement(nsETCPv2 + "AcceptServerName", "false")
												)
											)
										)
									)
								)
							)
						)
					)
				);

			// namespace variable, value depends on Eap type
			XNamespace nsEapType = "";
			// gets xml element to add values to
			XElement eapElement = newProfile.Element(nsWLAN + "MSM")
										.Element(nsWLAN + "security")
										.Element(nsOneX + "OneX")
										.Element(nsOneX + "EAPConfig")
										.Element(nsEHC + "EapHostConfig")
										.Element(nsEHC + "Config")
										.Element(nsBECP + "Eap");

			if (eap == 13)
			{
				// sets namespace
				nsEapType = nsETCPv1;

				// adds TLS specific xml elements
				eapElement.Add(
					new XElement(nsETCPv1 + "EapType",
						new XElement(nsETCPv1 + "CredentialsSource",
							new XElement(nsETCPv1 + "CertificateStore",
								new XElement(nsETCPv1 + "SimpleCertSelection", "true")
							)
						),
						new XElement(nsETCPv1 + "ServerValidation",
							new XElement(nsETCPv1 + "DisableUserPromptForServerValidation", "false")
						),
						new XElement(nsETCPv1 + "DifferentUsername", "false"),
						new XElement(nsETCPv2 + "PerformServerValidation", "true"),
						new XElement(nsETCPv2 + "AcceptServerName", "false")
					)
				);
			}
			else if (eap == 25)
			{
				// sets namespace
				nsEapType = nsMPCPv1;

				// adds MSCHAPv2 specific elements
				eapElement.Add(
					new XElement(nsMPCPv1 + "EapType",
						new XElement(nsMPCPv1 + "ServerValidation",
							new XElement(nsMPCPv1 + "DisableUserPromptForServerValidation", "false")
						),
						new XElement(nsMPCPv1 + "FastReconnect", "true"),
						new XElement(nsMPCPv1 + "InnerEapOptional", "false"),
						new XElement(nsBECP + "Eap",
							new XElement(nsBECP + "Type", "26"),
							new XElement(nsMCCP + "EapType",
								new XElement(nsMCCP + "UseWinLogonCredentials", "false")
							)
						),
						new XElement(nsMPCPv1 + "EnableQuarantineChecks", "false"),
						new XElement(nsMPCPv1 + "RequireCryptoBinding", "false"),
						new XElement(nsMPCPv1 + "PeapExtensions",
							new XElement(nsMPCPv2 + "PerformServerValidation", "true"),
							new XElement(nsMPCPv2 + "AcceptServerName", "true"),
							new XElement(nsMPCPv2 + "PeapExtensionsV2",
								new XElement(nsMPCPv3 + "AllowPromptingWhenServerCANotFound", "true")
							)
						)
					)
				);
			}


			// if any thumbprints exist, add them to the profile
			if (thumbprints != null && thumbprints.Any())
			{
				// gets element where thumbprint child elements are to be created
				XElement serverValidationElement = eapElement.Element(nsEapType + "EapType")
															 .Element(nsEapType + "ServerValidation");

				// creates TrustedRootCA child elements and assigns thumbprint as value
				foreach (string thumb in thumbprints)
				{
					serverValidationElement.Add(new XElement(nsEapType + "TrustedRootCA", thumb));
				}
			}


			// newProfile.Save(@"C:\Users\lwerivel18\Desktop\testFileFromC#.xml");

			// returns xml as string
			return newProfile.ToString();
		}

}

}
