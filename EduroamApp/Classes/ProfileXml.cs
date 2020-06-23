using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EduroamApp
{
    /// <summary>
    /// Wireless profile XML generator.
    /// Can construct wireless profiles for the following EAP types:
    /// - TLS (13)
    /// - PEAP-MSCHAPv2 (25/26)
    /// - TTLS (21) [NOT YET FUNCTIONAL]
    /// </summary>
    class ProfileXml
    {        
        // Namespaces
        static readonly XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
        static readonly XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
        static readonly XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
        static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";

        static readonly XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";
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


        /// <summary>
        /// Generates wireless profile xml. Content depends on the EAP type.
        /// </summary>
        /// <param name="ssid">Name of SSID associated with profile.</param>
        /// <param name="eapType">Type of EAP.</param>
        /// <param name="serverNames">Server names.</param>
        /// <param name="thumbprints">List of CA thumbprints, in hex</param>
        /// <returns>Complete wireless profile xml as string.</returns>
        public static string CreateProfileXml(string ssid, EapType eapType, string serverNames, List<string> thumbprints)
        {
            // sets AuthorId to 311 if using eap type TTLS
            int authId = eapType == EapType.TTLS ? 311 : 0;

            // hotspot2.0 domain is EapConfig.InstitutionInfo.InstId

            // creates common xml elements
            XElement newProfile =
                new XElement(nsWLAN + "WLANProfile",
                    new XElement(nsWLAN + "name", ssid),
                    new XElement(nsWLAN + "SSIDConfig",
                        new XElement(nsWLAN + "SSID",
                            new XElement(nsWLAN + "name", ssid)
                        ),
                        new XElement(nsWLAN + "nonBroadcast", "false")
                    ),
                    new XElement(nsWLAN + "connectionType", "ESS"),
                    new XElement(nsWLAN + "connectionMode", "auto"),
                    new XElement(nsWLAN + "autoSwitch", "false"),
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
                                            new XElement(nsEC + "Type", (uint)eapType),
                                            new XElement(nsEC + "VendorId", 0),
                                            new XElement(nsEC + "VendorType", 0),
                                            new XElement(nsEC + "AuthorId", authId)
                                        ),
                                        new XElement(nsEHC + "Config")
                                    )
                                )
                            )
                        )
                    )
                );

            // namespace variable, value depends on Eap type
            XNamespace nsEapType = "";
            string thumbprintNode = "";
            // gets xml element to add values to
            XElement configElement = newProfile
                .Element(nsWLAN + "MSM")
                .Element(nsWLAN + "security")
                .Element(nsOneX + "OneX")
                .Element(nsOneX + "EAPConfig")
                .Element(nsEHC + "EapHostConfig")
                .Element(nsEHC + "Config");
            
            if (eapType == EapType.TLS)
            {
                // sets namespace
                nsEapType = nsETCPv1;
                // sets name of thumbprint node
                thumbprintNode = "TrustedRootCA";

                // adds TLS specific xml elements
                configElement.Add(
                    new XElement(nsBECP + "Eap", 
                        new XElement(nsBECP + "Type", (uint)eapType),
                        new XElement(nsETCPv1 + "EapType",
                            new XElement(nsETCPv1 + "CredentialsSource",
                                new XElement(nsETCPv1 + "CertificateStore",
                                    new XElement(nsETCPv1 + "SimpleCertSelection", "true")
                                )   
                            ),
                            new XElement(nsETCPv1 + "ServerValidation",
                                new XElement(nsETCPv1 + "DisableUserPromptForServerValidation", "false"),
                                new XElement(nsETCPv1 + "ServerNames", serverNames)
                            ),
                            new XElement(nsETCPv1 + "DifferentUsername", "false"),
                            new XElement(nsETCPv2 + "PerformServerValidation", "true"),
                            new XElement(nsETCPv2 + "AcceptServerName", "false"),
                            new XElement(nsETCPv2 + "TLSExtensions", 
                                new XElement(nsETCPv3 + "FilteringInfo", 
                                    new XElement(nsETCPv3 + "CAHashList", new XAttribute("Enabled", "true"))
                                )
                            )
                        )
                    )
                );
            }
            else if (eapType == EapType.PEAP)
            {
                // sets namespace
                nsEapType = nsMPCPv1;
                // sets name of thumbprint node
                thumbprintNode = "TrustedRootCA";

                // adds MSCHAPv2 specific elements
                configElement.Add(
                    new XElement(nsBECP + "Eap",
                        new XElement(nsBECP + "Type", (uint)eapType),
                        new XElement(nsMPCPv1 + "EapType",
                            new XElement(nsMPCPv1 + "ServerValidation",
                                new XElement(nsMPCPv1 + "DisableUserPromptForServerValidation", "true"),
                                new XElement(nsMPCPv1 + "ServerNames", serverNames)
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
                                    new XElement(nsMPCPv3 + "AllowPromptingWhenServerCANotFound", "true") // TODO: should be false?
                                )
                            )
                        )
                    )
                );
            }
            // WORK IN PROGRESS - Dependent on setting correct user data for TTLS
            else if (eapType == EapType.TTLS)
            {
                // sets namespace
                nsEapType = nsTTLS;
                // sets name of thumbprint node
                thumbprintNode = "TrustedRootCAHash";

                configElement?.Add(
                    new XElement(nsTTLS + "EapTtls",
                        new XElement(nsTTLS + "ServerValidation",
                            new XElement(nsTTLS + "ServerNames", serverNames),
                            new XElement(nsTTLS + "DisablePrompt", "false")
                        ),
                        new XElement(nsTTLS + "Phase2Authentication"/*,
                            new XElement(nsEHC + "EapHostConfig",
                                new XElement(nsEHC + "EapMethod",
                                    new XElement(nsEC + "Type", 26),
                                    new XElement(nsEC + "VendorId", 0),
                                    new XElement(nsEC + "VendorType", 0),
                                    new XElement(nsEC + "AuthorId", 0)
                                ),
                                new XElement(nsEHC + "Config",
                                    new XElement(nsBECP + "Eap",
                                        new XElement(nsBECP + "Type", 26),
                                        new XElement(nsMCCP + "EapType",
                                            new XElement(nsMCCP + "UseWinLogonCredentials", "false")
                                        )
                                    )
                                )
                            )
                        */),
                        new XElement(nsTTLS + "Phase1Identity",
                            new XElement(nsTTLS + "IdentityPrivacy", "true"),
                            new XElement(nsTTLS + "AnonymousIdentity", "user")
                        )
                    )
                );
            }

            // if any thumbprints exist, add them to the profile
            if (thumbprints.Any())
            {
                XElement serverValidationElement = eapType switch
                {
                    EapType.TTLS => configElement
                        .Element(nsTTLS + "EapTtls")
                        .Element(nsTTLS + "ServerValidation"),
                    _ => configElement
                        .Element(nsBECP + "Eap")
                        .Element(nsEapType + "EapType")
                        .Element(nsEapType + "ServerValidation"),
                };


                // Format CA thumbprints into xs:element type="hexBinary"
                List<string> formattedThumbprints = thumbprints
                    .Select(thumb => Regex.Replace(thumb, " ", ""))
                    .Select(thumb => Regex.Replace(thumb, ".{2}", "$0 "))
                    .Select(thumb => thumb.ToUpper())
                    .Select(thumb => thumb.Trim())
                    .ToList();

                // creates TrustedRootCA(/Hash) child elements and assigns thumbprint as value
                formattedThumbprints.ForEach(thumb =>
                    serverValidationElement.Add(new XElement(nsEapType + thumbprintNode, thumb)));

                if (eapType == EapType.TLS)
                {
                    XElement caHashListElement = configElement
                        .Element(nsBECP + "Eap")
                        .Element(nsEapType + "EapType")
                        .Element(nsETCPv2 + "TLSExtensions")
                        .Element(nsETCPv3 + "FilteringInfo")
                        .Element(nsETCPv3 + "CAHashList");

                    // creates IssuerHash child elements and assigns thumbprint as value
                    formattedThumbprints.ForEach(thumb =>
                        caHashListElement.Add(new XElement(nsETCPv3 + "IssuerHash", thumb)));
                }
            }
            
            // returns xml as string
            return newProfile.ToString();
        }
    }

}
