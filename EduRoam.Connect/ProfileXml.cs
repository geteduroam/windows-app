using DocumentFormat.OpenXml;

using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace EduRoam.Connect
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
    public class ProfileXml
    {
        // Namespaces:

        // WLANProfile
        private static readonly XNamespace nsWLAN = "http://www.microsoft.com/networking/WLAN/profile/v1";
        private static readonly XNamespace nsOneX = "http://www.microsoft.com/networking/OneX/v1";
        private static readonly XNamespace nsEHC = "http://www.microsoft.com/provisioning/EapHostConfig";
        private static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
        private static readonly XNamespace nsBECP = "http://www.microsoft.com/provisioning/BaseEapConnectionPropertiesV1";

        private static readonly XNamespace nsHSP = "http://www.microsoft.com/networking/WLAN/HotspotProfile/v1";

        // TLS specific
        private static readonly XNamespace nsETCPv1 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV1";
        private static readonly XNamespace nsETCPv2 = "http://www.microsoft.com/provisioning/EapTlsConnectionPropertiesV2";

        // MSCHAPv2 specific
        private static readonly XNamespace nsMPCPv1 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV1";
        private static readonly XNamespace nsMPCPv2 = "http://www.microsoft.com/provisioning/MsPeapConnectionPropertiesV2";
        private static readonly XNamespace nsMCCP = "http://www.microsoft.com/provisioning/MsChapV2ConnectionPropertiesV1";

        // TTLS specific
        private static readonly XNamespace nsTTLS = "http://www.microsoft.com/provisioning/EapTtlsConnectionPropertiesV1";

        private static readonly string[] PREFERRED_SSIDS = new string[] { "eduroam", "govroam" };

        internal static ValueTuple<string, string> CreateSSIDProfileXml(AuthenticationMethod authMethod, string ssid)
            => CreateProfileXml(authMethod, withSSID: ssid);
        internal static ValueTuple<string, string> CreateHS20ProfileXml(AuthenticationMethod authMethod)
            => CreateProfileXml(authMethod, withHS20: true);

        /// <summary>
        /// Generates wireless profile xml. Content depends on the EAP type.
        /// </summary>
        /// <param name="authMethod">authMethod</param>
        /// <param name="withSSID">TODO</param>
        /// <param name="withHS20">If to install as hotspot 2.0 profile or not (separate profile from normal eap)</param>
        /// <returns>A tuple containing the profile name and the WLANProfile XML data</returns>
        private static ValueTuple<string, string> CreateProfileXml(
            AuthenticationMethod authMethod,
            string? withSSID = null,
            bool withHS20 = false,
            bool hiddenNetwork = false)
        {
            if (withHS20 && withSSID != null)
            {
                throw new ArgumentException("Cannot configure with both SSID and HS20"); // we can, but the result is confusing
            }

            if (withSSID != null && !authMethod.IsSSIDSupported)
            {
                throw new ArgumentException("Cannot configure " + nameof(authMethod) + " with SSID because it doesn't support SSID configuration");
            }

            if (withHS20 && !authMethod.IsHS20Supported)
            {
                throw new ArgumentException("Cannot configure " + nameof(authMethod) + " with Hotspot 2.0 because it doesn't support Hotspot 2.0 configuration");
            }

            if (withSSID != null && !authMethod.SSIDs.Any((ssid) => withSSID == ssid))
            {
                throw new ArgumentException("The ssid is not used by the authentication method");
            }

            if (authMethod.ServerNames.Count == 0 || authMethod.ServerCertificateAuthorities.Count == 0)
            {
                throw new ArgumentException("The authentication method must have server certificate validation through server name and allowed CA");
            }

            // Decide the profile name, which is the unique identifier for this profile
            var profileName = string.Empty;
            if (withHS20 && string.IsNullOrWhiteSpace(profileName))
            {
                profileName = authMethod.EapConfig?.InstitutionInfo.DisplayName ?? string.Empty;
            }

            if (withSSID != null && string.IsNullOrWhiteSpace(profileName))
            {
                profileName = withSSID;
            }

            if (string.IsNullOrWhiteSpace(profileName))
            {
                foreach (var preferredSSID in PREFERRED_SSIDS)
                {
                    if (authMethod.SSIDs.Contains(preferredSSID))
                    {
                        profileName = preferredSSID;
                        break;
                    }
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
            if (withHS20 && !string.IsNullOrWhiteSpace(profileName) && authMethod.SSIDs.Contains(profileName))
            {
                // since profileName is the unique identifier of the profile. avoid collisions with the profiles per ssid
                profileName += " via Passpoint"; // GEANT convention as fallback
            }

            // Construct XML document
            XElement ssidConfigElement;

            var newProfile =
                new XElement(nsWLAN + "WLANProfile",
                    new XElement(nsWLAN + "name", profileName),
                    ssidConfigElement =
                    new XElement(nsWLAN + "SSIDConfig"),
                    withHS20 ? GetHotspot2Element(authMethod) : null,
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
                                //new XElement(nsOneX + "cacheUserData", "true"),
                                new XElement(nsOneX + "authMode", "user"), // user
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

            var profileXml = newProfile.ToString();

            if (!validateXml(profileXml, "WLANProfile-v1.xsd"))
            {
                throw new WLANProfileException("WLAN profile (xml) is invalid");
            }

            return (profileName, profileXml);
        }

        private static XElement? GetHotspot2Element(AuthenticationMethod authMethod)
        {
            XElement roamingConsortiumElement;

            var hs20Element = new XElement(nsWLAN + "Hotspot2",
                new XElement(nsWLAN + "DomainName", authMethod.EapConfig?.InstitutionInfo.InstId),
                //new XElement(nsWLAN + "NAIRealm", ), // A list of Network Access Identifier (NAI) Realm identifiers. Entries in this list are usually of the form user@domain.
                // new XElement(nsWLAN + "Network3GPP", ), // A list of Public Land Mobile Network (PLMN) IDs.
                roamingConsortiumElement =
                new XElement(nsWLAN + "RoamingConsortium") // A list of Organizationally Unique Identifiers (OUI) assigned by IEEE.
            );

            authMethod.ConsortiumOIDs.ForEach(oui =>
                roamingConsortiumElement.Add(
                    new XElement(nsWLAN + "OUI", oui)
                )
            );

            return hs20Element;
        }

        private static XElement CreateEapConfiguration(
            EapType eapType,
            InnerAuthType innerAuthType,
            string? outerIdentity,
            List<string> serverNames,
            List<string> caThumbprints)
        {
            // creates the root xml strucure, with references to some of its descendants
            XElement configElement;
            var eapConfiguration =
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

            if ((eapType, innerAuthType) == (EapType.TLS, InnerAuthType.None))
            {
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
                            GetServerValidationElement(nsETCPv1, serverNames, caThumbprints),
                            new XElement(nsETCPv1 + "DifferentUsername", "false")
                        )
                    )
                );
            }
            else if ((eapType, innerAuthType) == (EapType.MSCHAPv2, InnerAuthType.None))
            {
                // MSCHAPv2 as outer EAP type should only be used in a TTLS tunnel
                // It does not support server validation
                if (serverNames.Any() || caThumbprints.Any())
                {
                    throw new EduroamAppUserException("not supported",
                        "MSCHAPv2 as outer EAP does not support server validation");
                }

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
                // Windows wants to add the realm itself, we must only set the local part
                // This appears to be the case for PEAP-EAP-MSCHAPv2
                var anonymousUserName = !string.IsNullOrEmpty(outerIdentity) && outerIdentity.Contains('@')
                    ? outerIdentity.Substring(0, outerIdentity.IndexOf("@"))
                    : outerIdentity
                    ;

                // adds MSCHAPv2 specific elements (inner eap)
                configElement.Add(
                    new XElement(nsBECP + "Eap", // PEAP
                        new XElement(nsBECP + "Type", (int)eapType),
                        new XElement(nsMPCPv1 + "EapType",
                            GetServerValidationElement(nsMPCPv1, serverNames, caThumbprints),
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
                                string.IsNullOrWhiteSpace(anonymousUserName)
                                    ? new XElement(nsMPCPv2 + "IdentityPrivacy",
                                        new XElement(nsMPCPv2 + "EnableIdentityPrivacy", "false")
                                    )
                                    : new XElement(nsMPCPv2 + "IdentityPrivacy",
                                        new XElement(nsMPCPv2 + "EnableIdentityPrivacy", "true"),
                                        new XElement(nsMPCPv2 + "AnonymousUserName", anonymousUserName)
                                    )
                            )
                        )
                    )
                );
            }
            else if (eapType == EapType.TTLS)
            {
                configElement.Add(
                    new XElement(nsTTLS + "EapTtls",
                        GetServerValidationElement(nsTTLS, serverNames, caThumbprints),
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
                                        eapType: EapType.MSCHAPv2,
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
                        string.IsNullOrWhiteSpace(outerIdentity)
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

            return eapConfiguration;
        }

        /// <summary>
        /// Create the XML node for server validation, verifying the CA by thumbprint and the server certificate by CN or subjectAltName
        /// </summary>
        /// <param name="ns">The namespace for this server validation element; this depends on the authentication method, valid values are currently nsETCPv1, nsMPCPv1 and nsTTLS</param>
        /// <param name="serverNames">List of server names, at least one of these must match the CN or subjectAltName of the certificate from the RADIUS server</param>
        /// <param name="caThumbprints">List of trusted CA thumbprints; the server certificate must be signed by one of these roots</param>
        private static XElement GetServerValidationElement(XNamespace ns, List<string> serverNames, List<string> caThumbprints)
        {
            // Windows uses different XML namespaces for different authentication methods,
            // and they are not completely consistent with naming across these different namespaces
            var thumbprintNodeName = ns == nsTTLS ? "TrustedRootCAHash" : "TrustedRootCA";
            var disablePromptNodeName = ns == nsTTLS ? "DisablePrompt" : "DisableUserPromptForServerValidation";

            var serverValidationElement = new XElement(ns + "ServerValidation",
                new XElement(ns + disablePromptNodeName, "true"),
                new XElement(ns + "ServerNames", string.Join(";", serverNames))
            );
            caThumbprints.ForEach(thumb =>
                serverValidationElement.Add(new XElement(ns + thumbprintNodeName, thumb.ToHexString())));

            return serverValidationElement;
        }

        private static bool validateXml(string xmlContent, string xsdResource)
        {
            var xsdContent = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EduRoam.Connect.{xsdResource}");

            var isValid = true;
            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CheckCharacters = true;
            settings.Schemas.Add(null, XmlReader.Create(xsdContent));
            settings.ValidationEventHandler += (sender, e) =>
            {
                isValid = false;
            };

            using (var reader = XmlReader.Create(new StringReader(xmlContent), settings))
            {
                while (reader.Read()) { }
            }

            return isValid;
        }

        /// <summary>
        /// Use this to determine if the authMethod can be installed as a WLanProfile
        /// </summary>
        /// <param name="authMethod"></param>
        /// <returns></returns>
        public static bool IsSupported(AuthenticationMethod authMethod)
        {
            // check if it has a supported
            if (authMethod.EapConfig != null && authMethod.EapConfig.CredentialApplicabilities
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
