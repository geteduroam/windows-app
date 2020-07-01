using System;
using System.Collections.Generic;
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

        static readonly XNamespace nsHSP = "http://www.microsoft.com/networking/WLAN/HotspotProfile/v1";

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
        /// <param name="authMethod">authMethod</param>
        /// <param name="withSsid">TODO</param>
        /// <param name="asHs2Profile">If to install as hotspot 2.0 profile or not (separate profile from normal eap)</param>
        /// <returns>A tuple containing the profile name and the WLANProfile XML data</returns>
        public static ValueTuple<string, string> CreateProfileXml(
            EapConfig.AuthenticationMethod authMethod,
            string withSsid = null,
            bool asHs2Profile = false)
        {
            // Get list of SSIDs to configure into profile
            List<string> ssids = withSsid != null
                ? new List<string> { withSsid }
                : authMethod.EapConfig.CredentialApplicabilities
                    .Where(cred => cred.NetworkType == IEEE802x.IEEE80211) // TODO: Wired 802.1x
                    .Where(cred => cred.MinRsnProto != "TKIP") // too insecure. TODO: test user experience
                    .Where(cred => cred.Ssid != null) // hs2 oid entires has no ssid
                    .Select(cred => cred.Ssid)
                    .ToList();

            // Get list of ConsortiumOIDs
            List<string> consortiumOids = authMethod.EapConfig.CredentialApplicabilities
                .Where(cred => cred.ConsortiumOid != null)
                .Select(cred => cred.ConsortiumOid)
                .ToList();

            // Decide the profile name
            var profileName = asHs2Profile
                ? (authMethod.EapConfig.InstitutionInfo.DisplayName)
                : ssids.FirstOrDefault() ?? EduroamNetwork.DefaultSsid;
            
            if (!ssids.Any())
                throw new EduroamAppUserError("no valid ssids in config");
            if (asHs2Profile && !SupportsHs2(authMethod))
                throw new EduroamAppUserError("hotspot2.0 not supported by authentication method");

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
                                        authMethod.EapType,
                                        authMethod.InnerAuthType,
                                        authMethod.ClientOuterIdentity,
                                        authMethod.ServerNames,
                                        authMethod.CertificateAuthoritiesAsX509Certificate2()
                                            .Select(cert => cert.Thumbprint).ToList())
                                )
                            )
                        )
                    )
                );


            // Add all the supported SSIDs
            foreach (var ssid in ssids) // xml schema allows for 256 occurrances max
            {
                ssidConfigElement.Add(
                    new XElement(nsWLAN + "SSID",
                        //new XElement(nsWLAN + "hex", ),
                        new XElement(nsWLAN + "name", ssid)
                    )
                );
            }
            ssidConfigElement.Add(
                new XElement(nsWLAN + "nonBroadcast", "true")
            );

            // Populate Hs2 fields
            foreach (string oui in consortiumOids)
            {
                roamingConsortiumElement.Add(
                    new XElement(nsWLAN + "OUI", oui)
                );
            }
            if (!asHs2Profile) hs2Element.Remove();

            // returns xml as string
            return (profileName, newProfile.ToString());
        }
        
        private static XElement CreateEapConfiguration(
            EapType eapType,
            InnerAuthType innerAuthType,
            string outerIdentity,
            List<string> serverNames,
            List<string> caThumbprints)
        {
            bool enableServerValidation = serverNames.Any() || caThumbprints.Any();
            
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
            // TODO: test TTLS PAP on someone elses computer
            // TODO: test TTLS EAP MSCHAPv2 on someone elses computer
            if ((eapType, innerAuthType) == (EapType.TLS, InnerAuthType.None))
            {
                // sets namespace and name of thumbprint node
                nsEapType = nsETCPv1;
                thumbprintNodeName = "TrustedRootCA";

                // adds TLS specific xml elements
                configElement.Add(
                    new XElement(nsBECP + "Eap", // TLS
                        new XElement(nsBECP + "Type", (uint)eapType),
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
                            new XElement(nsETCPv1 + "DifferentUsername", "false"), // TODO: outerIdentity
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
                    throw new EduroamAppUserError("not supported",
                        "MSCHAPv2 as outer EAP does bit support server validation");
                nsEapType = null;
                thumbprintNodeName = null;
                serverValidationElement = null;

                // adds MSCHAPv2 specific elements (inner eap)
                configElement.Add(
                    new XElement(nsBECP + "Eap", // MSCHAPv2
                        new XElement(nsBECP + "Type", (uint)eapType),
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

                // TODO: outerIdentity

                // adds MSCHAPv2 specific elements (inner eap)
                configElement.Add(
                    new XElement(nsBECP + "Eap", // PEAP
                        new XElement(nsBECP + "Type", (uint)eapType),
                        new XElement(nsMPCPv1 + "EapType",
                            serverValidationElement =
                            new XElement(nsMPCPv1 + "ServerValidation",
                                new XElement(nsMPCPv1 + "DisableUserPromptForServerValidation", enableServerValidation ? "true" : "false"),
                                new XElement(nsMPCPv1 + "ServerNames", string.Join(";", serverNames))
                            ),
                            new XElement(nsMPCPv1 + "FastReconnect", "true"),
                            new XElement(nsMPCPv1 + "InnerEapOptional", "false"),
                            new XElement(nsBECP + "Eap", // MSCHAPv2
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
                                InnerAuthType.EAP_PEAP_MSCHAPv2 =>
                                    CreateEapConfiguration(
                                        EapType.PEAP,
                                        InnerAuthType.EAP_MSCHAPv2,
                                        outerIdentity,
                                        serverNames, // strip server names from inner eap? remove this case altogether?
                                        caThumbprints
                                    ),
                                InnerAuthType.EAP_MSCHAPv2 =>
                                    CreateEapConfiguration(
                                        EapType.MSCHAPv2,
                                        InnerAuthType.None,
                                        outerIdentity,
                                        new List<string>(),
                                        new List<string>()
                                    ),
                                _ =>
                                    throw new EduroamAppUserError("unsupported auth method"),
                            }
                        ),
                        new XElement(nsTTLS + "Phase1Identity",
                            new XElement(nsTTLS + "IdentityPrivacy", "true"), // TODO, based on outerIdentity being null?
                            new XElement(nsTTLS + "AnonymousIdentity", outerIdentity ?? "")
                        )
                    )
                );
            }
            else
            {
                throw new EduroamAppUserError("unsupported auth method");
            }

            // Server validation
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
        /// Use this to determine if authMethod supports Hotspot 2.0
        /// </summary>
        public static bool SupportsHs2(EapConfig.AuthenticationMethod authMethod)
        {
            // TODO: hotspot2.0 requires Windows 10
            bool hasOID = authMethod.EapConfig.CredentialApplicabilities
                .Any(cred => cred.ConsortiumOid != null);
            bool isPEAP = authMethod.EapType == EapType.PEAP;
            return hasOID && !isPEAP;
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

        public static bool IsSupported(EapType eapType, InnerAuthType innerAuthType)
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
                (EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => at_least_win10,
                (EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => at_least_win10,
                _ => false,
            };
        }
    }

}
