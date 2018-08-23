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
                            new XAttribute(XNamespace.Xmlns + "eapTtls", nsTTLS),
                            new XElement(nsTTLS + "eapTtls",
                                new XElement(nsTTLS + "Username", uname),
                                new XElement(nsTTLS + "Password", pword),
                                new XElement(nsBEUP + "Eap",
                                    new XElement(nsBEUP + "Type", 21)
                                )
                            )
                        )
                    );
            }
            /*else if (eapType == 13)
            {
                newUserData =
                    new XElement(nsEHUC + "EapHostUserCredentials",
                        new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
                        new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
                        new XElement(nsEHUC + "EapMethod",
                            new XElement(nsEC + "Type", "13"),
                            new XElement(nsEC + "AuthorId", "0")
                        ),
                        new XElement(nsEHUC + "Credentials",
                            new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
                            new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
                            new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
                            new XAttribute(XNamespace.Xmlns + "eapTls", nsTLS),
                            new XElement(nsBEUP + "Eap",
                                new XElement(nsBEUP + "Type", "13"),
                                new XElement(nsTLS + "EapType",
                                    new XElement(nsTLS + "Username", ""),
                                    new XElement(nsTLS + "UserCert", "64 2a 67 25 ba 87 81 dd db 49 74 a3 ea 83 35 cf f4 5c 23 cf ")
                                )
                            )
                        )
                    );
            }*/
            
            // returns xml as string if not null
            return newUserData != null ? newUserData.ToString() : "";
        }
        
    }
}
