using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;

namespace EduroamApp
{
	class ProfileXml
	{
		// Properties
		public string Ssid { get; set; } = "eduroam";
		public List<string> Thumbprints { get; set; } = new List<string>();
		public uint Eap { get; set; } = 13;
		// enumerator for EAP types
		public enum EapType
		{
			TLS = 13,
			PEAP_MSCHAPv2 = 25
		}


		// Constructor
		public ProfileXml(string ssid, List<string> tPrints, EapType eapType)
		{
			Ssid = ssid;
			Thumbprints = tPrints;
			Eap = (uint)eapType; // converts to uint value
		}

		// Namespaces
		private XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
		private XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
		private XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
		private XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		private XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
			// TLS specific
		private XNamespace nsETCPv1 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";
		private XNamespace nsETCPv2 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV2";
			// MSCHAPv2 specific
		private XNamespace nsMPCPv1 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";
		private XNamespace nsMPCPv2 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2";
		private XNamespace nsMPCPv3 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV3";
		private XNamespace nsMCCP = "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1";


		public string CreateProfileXml()
		{
			XElement newProfile =
				new XElement(nsWLAN + "WLANProfile",
					new XElement(nsWLAN + "name", Ssid),
					new XElement(nsWLAN + "SSIDConfig",
						new XElement(nsWLAN + "SSID",
							new XElement(nsWLAN + "name", Ssid)
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
											new XElement(nsEC + "Type", Eap),
											new XElement(nsEC + "VendorId", 0),
											new XElement(nsEC + "VendorType", 0),
											new XElement(nsEC + "AuthorId", 0)),
										new XElement(nsEHC + "Config",
											new XElement(nsBECP + "Eap",
												new XElement(nsBECP + "Type", Eap),
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

			// if any thumbprints exist, add them to the profile
			if (Thumbprints.Any())
			{
				// gets element where thumbprint child elements are to be created
				XElement serverValidationElement = newProfile.Element(nsWLAN + "MSM")
									 .Element(nsWLAN + "security")
									 .Element(nsOneX + "OneX")
									 .Element(nsOneX + "EAPConfig")
									 .Element(nsEHC + "EapHostConfig")
									 .Element(nsEHC + "Config")
									 .Element(nsBECP + "Eap")
									 .Element(nsETCPv1 + "EapType")
									 .Element(nsETCPv1 + "ServerValidation");

				// creates TrustedRootCA child elements and assigns thumbprint as value
				foreach (string thumb in Thumbprints)
				{
					serverValidationElement.Add(new XElement(nsETCPv1 + "TrustedRootCA", thumb));
				}
			}


			newProfile.Save(@"C:\Users\lwerivel18\Desktop\testFileFromC#.xml");
			return "";
		}


		// takes in parameters as properties: ssid, profilename, EAP type, thumbprints

		// methods: depending on EAP type, build XML document

		/*
		// loads the XML file from its file path
		XDocument doc = XDocument.Load(xmlFile);

		// shortens namespaces from XML file for easier typing
		XNamespace ns1 = "http://www.microsoft.com/networking/WLAN/profile/v1";
		XNamespace ns2 = "http://www.microsoft.com/networking/OneX/v1";
		XNamespace ns3 = "http://www.microsoft.com/provisioning/EapHostConfig";
		XNamespace ns4 = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
		// namespace changes depending on EAP-type
		XNamespace ns5 = (cboMethod.SelectedIndex == 0 ? "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1" : "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1");

		// gets elements to edit
		XElement profileName = doc.Root.Element(ns1 + "name");

		XElement ssidName = doc.Root.Element(ns1 + "SSIDConfig")
								.Element(ns1 + "SSID")
								.Element(ns1 + "name");

		XElement serverValidationElement = doc.Root.Element(ns1 + "MSM")
								 .Element(ns1 + "security")
								 .Element(ns2 + "OneX")
								 .Element(ns2 + "EAPConfig")
								 .Element(ns3 + "EapHostConfig")
								 .Element(ns3 + "Config")
								 .Element(ns4 + "Eap")
								 .Element(ns5 + "EapType")
								 .Element(ns5 + "ServerValidation");
		//.Element(ns5 + "TrustedRootCA");

		// sets elements to desired values
		profileName.Value = ssid;
			ssidName.Value = ssid;

			foreach (string thumb in thumbprints)
			{
				serverValidationElement.Add(new XElement(ns5 + "TrustedRootCA", thumb));
			}

	// adds the xml declaration to the top of the document and converts it to string
	string wDeclaration = doc.Declaration.ToString() + Environment.NewLine + doc.ToString();
	*/

}

}
