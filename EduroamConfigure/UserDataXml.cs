using System.Xml.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// User data XML generator.
    /// Generates user data for the following EAP types:
    /// - PEAP-MSCHAPv2 (25/26)
    /// - TTLS (21) [NOT YET FUNCTIONAL]
    /// 
    /// Documentation:
    /// https://docs.microsoft.com/en-us/windows/win32/eaphost/eaphostusercredentialsschema-schema
    /// https://docs.microsoft.com/en-us/windows/win32/eaphost/user-profiles
    /// </summary>
    class UserDataXml
    {
        // Namespaces:

        static readonly XNamespace nsEHUC = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
        static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
        static readonly XNamespace nsBEMUC = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";
        static readonly XNamespace nsEUP = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
        static readonly XNamespace nsXSI = "http://www.w3.org/2001/XMLSchema-instance";
        static readonly XNamespace nsBEUP = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";
            
        // MSCHAPv2 specific
        static readonly XNamespace nsMPUP = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
        static readonly XNamespace nsMCUP = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";
        
        // TTLS specific
        static readonly XNamespace nsTTLS = "http://www.microsoft.com/provisioning/EapTtlsUserPropertiesV1";
        // TLS specific
        static readonly XNamespace nsTLS = "http://www.microsoft.com/provisioning/EapTtlsUserPropertiesV1";

        /// <summary>
        /// Generates user data xml.
        /// </summary>
        /// <param name="uname">Username.</param>
        /// <param name="pword">Password.</param>
        /// <param name="eapType">EAP type</param>
        /// <returns>Complete user data xml as string.</returns>
        public static string CreateUserDataXml(string uname, string pword, EapType eapType)
        {
            XElement newUserData = null;

            if (eapType == EapType.PEAP)
            {
                newUserData =
                    new XElement(nsEHUC + "EapHostUserCredentials",
                        new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
                        new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
                        new XElement(nsEHUC + "EapMethod",
                            new XElement(nsEC + "Type", (uint)EapType.PEAP),
                            new XElement(nsEC + "AuthorId", "0")
                        ),
                        new XElement(nsEHUC + "Credentials",
                            new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
                            new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
                            new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
                            new XAttribute(XNamespace.Xmlns + "MsPeap", nsMPUP),
                            new XAttribute(XNamespace.Xmlns + "MsChapV2", nsMCUP),
                            new XElement(nsBEUP + "Eap",
                                new XElement(nsBEUP + "Type", (uint)EapType.PEAP),
                                new XElement(nsMPUP + "EapType",
                                    new XElement(nsMPUP + "RoutingIdentity"),
                                    new XElement(nsBEUP + "Eap",
                                        new XElement(nsBEUP + "Type", "26"), // MSCHAPv2
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
            // TODO: WORK IN PROGRESS - Dependent on creating a correct profile XML for TTLS
            else if (eapType == EapType.TTLS)
            {
                newUserData =
                    new XElement(nsEHUC + "EapHostUserCredentials",
                        new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
                        new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
                        new XElement(nsEHUC + "EapMethod",
                            new XElement(nsEC + "Type", (uint)EapType.TTLS),
                            new XElement(nsEC + "AuthorId", "67532") // TODO: nani?
                        ),
                        new XElement(nsEHUC + "Credentials",
                            new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
                            new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
                            new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
                            new XAttribute(XNamespace.Xmlns + "eapTtls", nsTTLS),
                            new XElement(nsTTLS + "eapTtls",
                                new XElement(nsTTLS + "Username", uname),
                                new XElement(nsTTLS + "Password", pword),
                                new XElement(nsBEUP + "Eap",
                                    new XElement(nsBEUP + "Type", (uint)EapType.TTLS)
                                )
                            )
                        )
                    );
            }
            // TODO: handle the missing EapType cases in a different way?

            // returns xml as string if not null
            return newUserData != null ? newUserData.ToString() : "";
        }
        
    }
}
