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
		private static XNamespace nsEHUC = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
		private static XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
		private static XNamespace nsBEMUC = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";
		private static XNamespace nsEUP = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
		private static XNamespace nsXSI = "http://www.w3.org/2001/XMLSchema-instance";
		private static XNamespace nsBEUP = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";
		private static XNamespace nsMPUP = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
		private static XNamespace nsMCUP = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";

		/// <summary>
		/// Generates user data xml.
		/// </summary>
		/// <param name="uname">Username.</param>
		/// <param name="pword">Password.</param>
		/// <returns>Complete user data xml as string.</returns>
		public static string CreateUserDataXml(string uname, string pword)
		{
			XElement newUserData =
				new XElement(nsEHUC + "EapHostUserCredentials",
					new XElement(nsEHUC + "EapMethod",
						new XElement(nsEC + "Type", "25"),
						new XElement(nsEC + "AuthorId", "0")
					),
					new XElement(nsEHUC + "Credentials",
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
				);

			newUserData.Save(@"C:\Users\lwerivel18\Desktop\userDataFromC#.xml");
			return newUserData.ToString();
		}


	}
}
