using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Wireless profile XML generator.
    /// Can construct wireless profiles for the following EAP types:
    /// - TLS (13)
    /// - PEAP-MSCHAPv2 (25/26)
    /// - TTLS (21) [NOT YET FUNCTIONAL]
    ///
    /// Documentation of the XML format:
    ///     https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-elements
    ///     https://docs.microsoft.com/en-us/windows/win32/nativewifi/onexschema-elements
    ///     https://docs.microsoft.com/en-us/windows/win32/eaphost/eaptlsconnectionpropertiesv1schema-servervalidationparameters-complextype
    ///     https://docs.microsoft.com/en-us/powershell/module/vpnclient/new-eapconfiguration?view=win10-ps
    /// 
    /// </summary>
    class ProfileXml
    {
        // Namespaces:

        // WLANProfile
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
        /// <param name="innerAuthType">Type of inner auth method.</param>
        /// <param name="serverNames">Server names.</param>
        /// <param name="caThumbprints">List of CA thumbprints, in hex</param>
        /// <param name="disablePromptForServerValidation">Will cause the wifi profile to fail if server cannot be validated</param>
        /// <returns>Complete wireless profile xml as string.</returns>
        public static string CreateProfileXml(
            string ssid,
            EapType eapType,
            InnerAuthType innerAuthType,
            string serverNames,
            List<string> caThumbprints,
            bool disablePromptForServerValidation = true)
        {
            // hotspot2.0 domain is EapConfig.InstitutionInfo.InstId

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
                                    CreateEapConfiguration(eapType, innerAuthType, serverNames, caThumbprints, disablePromptForServerValidation)
                                )
                            )
                        )
                    )
                );

            // returns xml as string
            return newProfile.ToString();
        }
        
        private static XElement CreateEapConfiguration(
            EapType eapType,
            InnerAuthType innerAuthType,
            string serverNames,
            List<string> caThumbprints,
            bool disablePromptForServerValidation = true)
        {
            // creates the root xml strucure, with references to some of its descendants
            XElement configElement;
            XElement serverValidationElement;
            XElement caHashListElement = null; // eapType == eapType.TLS only
            XElement eapConfiguration =
                new XElement(nsEHC + "EapHostConfig",
                    new XElement(nsEHC + "EapMethod",
                        new XElement(nsEC + "Type", (uint)eapType),
                        new XElement(nsEC + "VendorId",   0),
                        new XElement(nsEC + "VendorType", 0),
                        new XElement(nsEC + "AuthorId", eapType == EapType.TTLS ? 311 : 0) // no geant link
                    ),
                    configElement =
                    new XElement(nsEHC + "Config")
                );


            // namespace element local names dependant on EAP type
            XNamespace nsEapType;
            string thumbprintNodeName;

            // TODO: test TLS
            // TODO: test PEAP on someone elses computer
            if (eapType == EapType.TLS)
            {
                // sets namespace and name of thumbprint node
                nsEapType = nsETCPv1;
                thumbprintNodeName = "TrustedRootCA";

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
                            serverValidationElement =
                            new XElement(nsETCPv1 + "ServerValidation",
                                new XElement(nsETCPv1 + "DisableUserPromptForServerValidation", disablePromptForServerValidation ? "true" : "false"),
                                new XElement(nsETCPv1 + "ServerNames", serverNames)
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
            else if ((eapType, innerAuthType) == (EapType.PEAP, InnerAuthType.EAP_MSCHAPv2))
            {
                // sets namespace and name of thumbprint node
                nsEapType = nsMPCPv1;
                thumbprintNodeName = "TrustedRootCA";

                // adds MSCHAPv2 specific elements (inner eap)
                configElement.Add(
                    new XElement(nsBECP + "Eap",
                        new XElement(nsBECP + "Type", (uint)eapType),
                        new XElement(nsMPCPv1 + "EapType",
                            serverValidationElement =
                            new XElement(nsMPCPv1 + "ServerValidation",
                                new XElement(nsMPCPv1 + "DisableUserPromptForServerValidation", disablePromptForServerValidation ? "true" : "false"),
                                new XElement(nsMPCPv1 + "ServerNames", serverNames)
                            ),
                            new XElement(nsMPCPv1 + "FastReconnect", "true"),
                            new XElement(nsMPCPv1 + "InnerEapOptional", "false"),
                            new XElement(nsBECP + "Eap",
                                new XElement(nsBECP + "Type", (uint)innerAuthType),
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
                    )
                );
            }
            // TODO: WORK IN PROGRESS - Dependent on setting correct user data for TTLS
            else if (eapType == EapType.TTLS)
            {
                // sets namespace and name of thumbprint node
                nsEapType = nsTTLS;
                thumbprintNodeName = "TrustedRootCAHash";

                configElement.Add(
                    new XElement(nsTTLS + "EapTtls",
                        serverValidationElement =
                        new XElement(nsTTLS + "ServerValidation",
                            new XElement(nsTTLS + "ServerNames", serverNames),
                            new XElement(nsTTLS + "DisablePrompt", "false") // TODO:  disablePromptForServerValidation ? "true" : "false"
                        ),
                        new XElement(nsTTLS + "Phase2Authentication",
                            innerAuthType switch
                            {
                                InnerAuthType.PAP =>
                                    new XElement(nsTTLS + "PAPAuthentication"),
                                InnerAuthType.MSCHAP =>
                                    new XElement(nsTTLS + "MSCHAPAuthentication"),
                                InnerAuthType.MSCHAPv2 =>
                                    new XElement(nsTTLS + "MSCHAPv2Authentication",
                                        new XElement(nsTTLS + "UseWinlogonCredentials", "false")
                                    ),
                                InnerAuthType.EAP_MSCHAPv2 =>
                                    CreateEapConfiguration(
                                        EapType.PEAP,
                                        InnerAuthType.EAP_MSCHAPv2,
                                        serverNames,
                                        caThumbprints,
                                        disablePromptForServerValidation
                                    ),
                                _ => throw new EduroamAppUserError("unsupported inner auth method"),
                            }
                        ),
                        new XElement(nsTTLS + "Phase1Identity",
                            new XElement(nsTTLS + "IdentityPrivacy", "true"),
                            new XElement(nsTTLS + "AnonymousIdentity", "user")
                        )
                    )
                );
            }
            else
            {
                throw new EduroamAppUserError("unsupported auth method");
            }

            // if any CA thumbprints exist, add them to the profile
            if (caThumbprints.Any())
            {
                // Format the CA thumbprints into xs:element type="hexBinary"
                List<string> formattedThumbprints = caThumbprints
                    .Select(thumb => Regex.Replace(thumb, " ", ""))
                    .Select(thumb => Regex.Replace(thumb, ".{2}", "$0 "))
                    .Select(thumb => thumb.ToUpper())
                    .Select(thumb => thumb.Trim())
                    .ToList();

                // Write the CA thumbprints to their proper places in the XML:

                formattedThumbprints.ForEach(thumb =>
                    serverValidationElement.Add(new XElement(nsEapType + thumbprintNodeName, thumb)));

                if (caHashListElement != null) // TLS only
                    formattedThumbprints.ForEach(thumb =>
                        caHashListElement.Add(new XElement(nsETCPv3 + "IssuerHash", thumb)));
            }

            return eapConfiguration;
        }
        
        public static bool IsSupported(EapConfig.AuthenticationMethod authMethod)
        {
            return IsSupported(authMethod.EapType, authMethod.InnerAuthType);
        }

        public static bool IsSupported(EapType eapType, InnerAuthType innerAuthType)
        {
            bool at_least_win10 = System.Environment.OSVersion.Version.Major >= 10;
            return (eapType, innerAuthType) switch
            {
                (EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) => true,
                (EapType.TLS, InnerAuthType.None) => true,
                (EapType.TTLS, InnerAuthType.PAP) => true,
                (EapType.TTLS, InnerAuthType.MSCHAP) => true,
                (EapType.TTLS, InnerAuthType.MSCHAPv2) => true,
                (EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => at_least_win10,
                _ => false,
            };
        }
    }

}
