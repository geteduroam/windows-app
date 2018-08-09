using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EduroamApp
{
	class UserDataXml
	{
		// Namespaces
		private static readonly XNamespace nsEHUC = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
		private static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		private static readonly XNamespace nsBEMUC = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";
		private static readonly XNamespace nsEUP = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
		private static readonly XNamespace nsXSI = "http://www.w3.org/2001/XMLSchema-instance";
		private static readonly XNamespace nsBEUP = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";
		private static readonly XNamespace nsMPUP = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
		private static readonly XNamespace nsMCUP = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";
			// TTLS specific
		static readonly XNamespace nsETUP = "http://www.microsoft.com/provisioning/EapTtlsUserPropertiesV1";

		/// <summary>
		/// Generates user data xml.
		/// </summary>
		/// <param name="uname">Username.</param>
		/// <param name="pword">Password.</param>
		/// <param name="eapType">EAP type</param>
		/// <returns>Complete user data xml as string.</returns>
		public static string CreateUserDataXml(string uname, string pword, uint eapType)
		{
			XElement newUserData = null;

			if (eapType == 25)
			{
				newUserData =
					new XElement(nsEHUC + "EapHostUserCredentials",
						new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
						new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
						new XElement(nsEHUC + "EapMethod",
							new XElement(nsEC + "Type", "25"),
							new XElement(nsEC + "AuthorId", "0")
						),
						new XElement(nsEHUC + "Credentials",
							new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
							new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
							new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
							new XAttribute(XNamespace.Xmlns + "MsPeap", nsMPUP),
							new XAttribute(XNamespace.Xmlns + "MsChapV2", nsMCUP),
							new XElement(nsBEUP + "Eap",
								new XElement(nsBEUP + "Type", "25"),
								new XElement(nsMPUP + "EapType",
									new XElement(nsMPUP + "RoutingIdentity"),
									new XElement(nsBEUP + "Eap",
										new XElement(nsBEUP + "Type", "26"),
										new XElement(nsMCUP + "EapType",
											new XElement(nsMCUP + "Username", uname),
											new XElement(nsMCUP + "Password", pword),
											new XElement(nsMCUP + "LogonDomain")
										)
									)
								)
							)
						)
					);
			}
			else if (eapType == 21)
			{
				newUserData =
					new XElement(nsEHUC + "EapHostUserCredentials",
						new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
						new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
						new XElement(nsEHUC + "EapMethod",
							new XElement(nsEC + "Type", "21"),
							new XElement(nsEC + "AuthorId", "67532")
						),
						new XElement(nsEHUC + "Credentials",
							new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
							new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
							new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
							new XAttribute(XNamespace.Xmlns + "eapTtls", nsETUP),
							new XElement(nsETUP + "eapTtls",
								new XElement(nsETUP + "Username", uname),
								new XElement(nsETUP + "Password", pword),
								new XElement(nsBEUP + "Eap",
									new XElement(nsBEUP + "Type", 21)
								)
							)
						)
					);
			}

			 // newUserData?.Save(@"C:\Users\lwerivel18\Desktop\userDataFromC#.xml");

			// returns xml as string if not null
			return newUserData.ToString();
		}

	}
}
