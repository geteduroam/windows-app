using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace EduroamConfigure
{
    /// <summary>
    /// Stores information found in an EAP-config file.
    /// </summary>
    public class EapConfig
    {
        // Properties
        public List<AuthenticationMethod> AuthenticationMethods { get; }
        public List<CredentialApplicability> CredentialApplicabilities { get; }
        public ProviderInfo InstitutionInfo { get; }


        // Constructor
        EapConfig(
            List<AuthenticationMethod> authenticationMethods,
            List<CredentialApplicability> credentialApplicabilities,
            ProviderInfo institutionInfo)
        {
            AuthenticationMethods = authenticationMethods;
            CredentialApplicabilities = credentialApplicabilities;
            InstitutionInfo = institutionInfo;

            AuthenticationMethods.ForEach(authMethod =>
            {
                authMethod.EapConfig = this;
            });
        }


        /// <summary>
        /// AuthenticationMethod contains information about client certificates and CAs.
        /// </summary>
        public class AuthenticationMethod
        {
            // Properties
            public EapConfig EapConfig { get; set; } // reference to parent EapConfig
            public EapType EapType { get; }
            public InnerAuthType InnerAuthType { get; }
            public List<string> CertificateAuthorities { get; } // base64 encoded DER certificate
            public List<string> ServerNames { get; }
            public string ClientCertificate { get; } // base64 encoded PKCS12 certificate+privkey bundle
            public string ClientCertificatePassphrase { get; } // passphrase for ^
            public string ClientOuterIdentity { get; } // also known as: anonymous identity, routing identity
            public string ClientInnerIdentitySuffix { get; } // realm
            public bool ClientInnerIdentityHint { get; } // Wether to disallow subrealms or not (see https://github.com/GEANT/CAT/issues/190)

            /// <summary>
            /// Converts and enumerates CertificateAuthorities as X509Certificate2 objects
            /// </summary>
            public IEnumerable<X509Certificate2> CertificateAuthoritiesAsX509Certificate2()
            {
                foreach (var ca in CertificateAuthorities)
                {
                    var cert = new X509Certificate2(Convert.FromBase64String(ca));

                    // sets the friendly name of certificate
                    if (string.IsNullOrEmpty(cert.FriendlyName))
                        cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);
                    yield return cert;
                }
            }

            /// <summary>
            /// Converts the client certificate base64 data to a X509Certificate2 object
            /// </summary>
            /// <returns>X509Certificate2 if any, otherwise null</returns>
            public X509Certificate2 ClientCertificateAsX509Certificate2()
            {
                if (string.IsNullOrEmpty(ClientCertificate))
                    return null;

                var cert = new X509Certificate2(
                    Convert.FromBase64String(ClientCertificate),
                    ClientCertificatePassphrase,
                    X509KeyStorageFlags.PersistKeySet);

                // sets the friendly name of certificate
                if (string.IsNullOrEmpty(cert.FriendlyName))
                    cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);

                return cert;
            }

            /// <summary>
            /// User presentable EAP scheme identitifer
            /// </summary>
            public string EapSchemeName()
            {
                if (InnerAuthType != InnerAuthType.None)
                    return EapType.ToString() + "_" + InnerAuthType.ToString();
                return EapType.ToString();
            }

            public bool NeedsLoginCredentials()
            {
                return EapType != EapType.TLS; // TODO: make this more maintainable
            }

            public bool NeedClientCertificate()
            {
                if (NeedsLoginCredentials()) return false;
                return string.IsNullOrEmpty(ClientCertificate);
            }

            // Constructor
            public AuthenticationMethod(
                EapType eapType,
                InnerAuthType innerAuthType,
                List<string> certificateAuthorities,
                List<string> serverName,
                string clientCertificate = null,
                string clientCertificatePassphrase = null,
                string clientOuterIdentity = null,
                string innerIdentitySuffix = null,
                bool innerIdentityHint = false
            )
            {
                EapType = eapType;
                InnerAuthType = innerAuthType;
                CertificateAuthorities = certificateAuthorities;
                ServerNames = serverName;
                ClientCertificate = clientCertificate;
                ClientCertificatePassphrase = clientCertificatePassphrase;
                ClientOuterIdentity = clientOuterIdentity;
                ClientInnerIdentitySuffix = innerIdentitySuffix;
                ClientInnerIdentityHint = innerIdentityHint;
            }
        }

        /// <summary>
        /// ProviderInfo contains information about the config file's provider.
        /// </summary>
        public class ProviderInfo
        {
            // Properties
            public string DisplayName { get; }
            public string Description { get; }
            public byte[] LogoData { get; }
            public string LogoMimeType { get; }
            public string EmailAddress { get; }
            public string WebAddress { get; }
            public string Phone { get; }
            public string InstId { get; }
            public string TermsOfUse { get; }
            public ValueTuple<double, double>? Location { get; } // nullable coordinates on the form (Latitude, Longitude)

            // Constructor
            public ProviderInfo(
                string displayName,
                string description,
                byte[] logoData,
                string logoMimeType,
                string emailAddress,
                string webAddress,
                string phone,
                string instId,
                string termsOfUse,
                ValueTuple<double, double>? Location)
            {
                DisplayName = displayName;
                Description = description;
                LogoData = logoData;
                LogoMimeType = logoMimeType;
                EmailAddress = emailAddress;
                WebAddress = webAddress;
                Phone = phone;
                InstId = instId;
                TermsOfUse = termsOfUse.Replace("\r\n", " "); // TODO: n-n-n-nani?
                this.Location = Location;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public class CredentialApplicability
        {
            public IEEE802x NetworkType { get; }

            // IEEE80211 only:
            public string Ssid { get; } // Wifi SSID, TODO: use
            public string ConsortiumOid { get; } // Hotspot2.0
            public string MinRsnProto { get; } // "TKIP" or "CCMP", TODO: use


            // IEEE8023 only:
            public string NetworkId { get; }

            private CredentialApplicability(
                IEEE802x networkType,
                string ssid,
                string consortiumOid,
                string minRsnProto,
                string networkId)
            {

                NetworkType = networkType;
                Ssid = ssid;
                ConsortiumOid = consortiumOid;
                MinRsnProto = minRsnProto;
                NetworkId = networkId;
            }

            public static CredentialApplicability IEEE80211(
                string ssid,
                string consortiumOid,
                string minRsnProto)
            {
                return new CredentialApplicability(
                    IEEE802x.IEEE80211,
                    ssid,
                    consortiumOid,
                    minRsnProto ?? "CCMP",
                    null);
            }

            public static CredentialApplicability IEEE8023(
                string networkId)
            {
                return new CredentialApplicability(
                    IEEE802x.IEEE8023,
                    null,
                    null,
                    null,
                    networkId);
            }

        }


        /// <summary>
        /// Creates a new EapConfig object from EAP config xml data
        /// </summary>
        /// <param name="eapConfigXmlData">EAP config XML as string</param>
        /// <returns>EapConfig object</returns>
        public static EapConfig FromXmlData(string eapConfigXmlData)
        {
            // XML format Documentation:
            // Current:  https://github.com/GEANT/CAT/blob/master/devices/eap_config/eap-metadata.xsd
            // Outdated: https://tools.ietf.org/id/draft-winter-opsawg-eap-metadata-00.html

            // TODO: Hotspot 2.0

            static Func<XElement, bool> nameIs(string name) => // shorthand lambda
                element => element.Name.LocalName == name;

            // load the XML file into a XElement object
            XElement eapConfigXml = XElement.Parse(eapConfigXmlData);
            foreach (XElement eapIdentityProvider in eapConfigXml.Descendants().Where(nameIs("EAPIdentityProvider")))
            {
                //TODO: yield return from this
            }

            // create a new empty list for authentication methods
            List<EapConfig.AuthenticationMethod> authMethods =
                new List<EapConfig.AuthenticationMethod>();

            // iterate over all AuthenticationMethods elements from xml
            foreach (XElement authMethodXml in eapConfigXml.Descendants().Where(nameIs("AuthenticationMethod")))
            {
                XElement serverSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ServerSideCredential"));
                XElement clientSideCredentialXml = authMethodXml
                    .Elements().FirstOrDefault(nameIs("ClientSideCredential"));

                // get EAP method type
                EapType eapType = (EapType)(uint)authMethodXml
                    .Elements().First(nameIs("EAPMethod"))
                    .Elements().First(nameIs("Type"));

                InnerAuthType innerAuthType = (InnerAuthType?)(uint?)authMethodXml
                    .Elements().FirstOrDefault(nameIs("InnerAuthenticationMethod"))
                    ?.Descendants().FirstOrDefault(nameIs("Type"))
                    ?? InnerAuthType.None;

                // ServerSideCredential

                // get list of strings of CA certificates
                List<string> certAuths = serverSideCredentialXml
                    .Elements().Where(nameIs("CA"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // get list of strings of server IDs
                List<string> serverNames = serverSideCredentialXml
                    .Elements().Where(nameIs("ServerID"))
                    .Select(xElement => (string)xElement)
                    .ToList();

                // ClientSideCredential

                // user certificate
                var clientCert = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("ClientCertificate"));
                var clientCertPasswd = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("Passphrase"));

                // inner/outer identity
                var clientOuterIdentity = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("OuterIdentity"));
                var clientInnerIdentitySuffix = (string)clientSideCredentialXml
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
                var clientInnerIdentityHint = (bool?)clientSideCredentialXml // TODO: will cast to bool work?
                    ?.Elements().FirstOrDefault(nameIs("InnerIdentityHint")) ?? false;

                // Translate erronous data from cat.eduroam.org: https://github.com/GEANT/CAT/pull/191
                // TODO: remove this when PR is merged and deployed!
                if ((eapType, innerAuthType) == (EapType.TTLS, InnerAuthType.EAP_MSCHAPv2))
                {
                    innerAuthType = InnerAuthType.MSCHAPv2;
                }

                // create new authentication method object and adds it to list
                authMethods.Add(new EapConfig.AuthenticationMethod(
                    eapType,
                    innerAuthType,
                    certAuths,
                    serverNames,
                    clientCert,
                    clientCertPasswd,
                    clientOuterIdentity,
                    clientInnerIdentitySuffix,
                    clientInnerIdentityHint
                ));
            }

            // create a new empty list for authentication methods
            List<EapConfig.CredentialApplicability> credentialApplicabilities =
                new List<EapConfig.CredentialApplicability>();

            foreach (XElement credentialApplicabilityXml in eapConfigXml.Descendants().First(nameIs("CredentialApplicability")).Elements())
            {
                credentialApplicabilities.Add(credentialApplicabilityXml.Name.LocalName switch
                {
                    "IEEE80211" =>
                        CredentialApplicability.IEEE80211(
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("SSID")),
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("ConsortiumOID")),
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("MinRSNProto"))
                        ),
                    "IEEE8023" =>
                        CredentialApplicability.IEEE8023(
                            (string)credentialApplicabilityXml.Elements().FirstOrDefault(nameIs("NetworkID"))
                        ),
                    _ => throw new NotImplementedException(),
                });
            }

            // get logo and identity element
            XElement logoElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderLogo"));
            XElement eapIdentityElement = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("EAPIdentityProvider")); // TODO: remove

            // get institution ID from identity element
            var instId = (string)eapIdentityElement.Attribute("ID"); // TODO: obsolete?

            // get provider's logo as base64 encoded string and its mime-type
            var logoData = Convert.FromBase64String((string)logoElement ?? "");
            var logoMimeType = (string)logoElement?.Attribute("mime");

            // Read ProviderInfo attributes:
            XElement providerInfoXml = eapConfigXml
                .Descendants().FirstOrDefault(nameIs("ProviderInfo"));

            var displayName = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("DisplayName"));
            var description = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Description"));
            var emailAddress = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("EmailAddress"));
            var webAddress = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("WebAddress"));
            var phone = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("Helpdesk"))
                ?.Elements().FirstOrDefault(nameIs("Phone"));
            var termsOfUse = (string)providerInfoXml
                ?.Elements().FirstOrDefault(nameIs("TermsOfUse"));

            // Read coordinates
            ValueTuple<double, double>? location = null;
            if (providerInfoXml?.Elements().Where(nameIs("ProviderLocation")).Any() ?? false)
            {
                location = (
                    (double)providerInfoXml.Descendants().First(nameIs("Latitude")),
                    (double)providerInfoXml.Descendants().First(nameIs("Longitude"))
                );
            }


            // create EapConfig object and adds the info
            return new EapConfig(
                authMethods,
                credentialApplicabilities,
                new EapConfig.ProviderInfo(
                    displayName ?? string.Empty,
                    description ?? string.Empty,
                    logoData,
                    logoMimeType ?? string.Empty,
                    emailAddress ?? string.Empty,
                    webAddress ?? string.Empty,
                    phone ?? string.Empty,
                    instId ?? string.Empty,
                    termsOfUse ?? string.Empty,
                    location)
            );
        }

    }

    public enum IEEE802x
    {
        /// <summary>
        /// Wired LAN
        /// </summary>
        IEEE8023, // TODO: add full support for this

        /// <summary>
        /// Wireless LAN
        /// </summary>
        IEEE80211
    }

    /// <summary>
    ///  https://www.vocal.com/secure-communication/eap-types/
    /// </summary>
    public enum EapType : uint
    {
        TLS = 13,
        TTLS = 21,
        PEAP = 25,
        MSCHAPv2 = 26,
    }

    public enum InnerAuthType: uint
    {
        // For those EAP types with no inner auth method (TLS and MSCHAPv2)
        None = 0,
        // Non-EAP methods
        PAP = 1,
        //CHAP = NaN, // Not defined in EapConfig schema
        MSCHAP = 2,
        MSCHAPv2 = 3,
        // Tunneled Eap methods
        EAP_PEAP_MSCHAPv2 = 25,
        EAP_MSCHAPv2 = 26,
    }

}
