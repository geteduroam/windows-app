using EduRoam.Connect.Eap;
using EduRoam.Connect.Exceptions;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EduRoam.Connect
{
    /// <summary>
    /// User data XML generator.
    /// </summary>
    /// <remarks>
    /// Documentation:
    ///
    /// https://docs.microsoft.com/en-us/windows/win32/eaphost/eaphostusercredentialsschema-schema
    /// https://docs.microsoft.com/en-us/windows/win32/eaphost/user-profiles
    /// https://github.com/rozmansi/WLANSetEAPUserData/tree/master/Examples
    /// C:\Windows\schemas\EAPMethods
    /// C:\Windows\schemas\EAPHost
    /// </remarks>
    public class UserDataXml
    {
        // Namespaces:

        private static readonly XNamespace nsEHUC = "http://www.microsoft.com/provisioning/EapHostUserCredentials";
        private static readonly XNamespace nsEC = "http://www.microsoft.com/provisioning/EapCommon";
        private static readonly XNamespace nsBEMUC = "http://www.microsoft.com/provisioning/BaseEapMethodUserCredentials";
        private static readonly XNamespace nsEUP = "http://www.microsoft.com/provisioning/EapUserPropertiesV1";
        private static readonly XNamespace nsXSI = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace nsBEUP = "http://www.microsoft.com/provisioning/BaseEapUserPropertiesV1";

        // PEAP / MSCHAPv2 specific
        private static readonly XNamespace nsMPUP = "http://www.microsoft.com/provisioning/MsPeapUserPropertiesV1";
        private static readonly XNamespace nsMCUP = "http://www.microsoft.com/provisioning/MsChapV2UserPropertiesV1";

        // TTLS specific
        private static readonly XNamespace nsTTLS = "http://www.microsoft.com/provisioning/EapTtlsUserPropertiesV1";

        // TLS specific
        private static readonly XNamespace nsTLS = "http://www.microsoft.com/provisioning/EapTlsUserPropertiesV1";

        /// <summary>
        /// Generates user data xml.
        /// </summary>
        /// <param name="authMethod">authMethod</param>
        /// <returns>Complete user data xml as string.</returns>
        internal static string CreateUserDataXml(AuthenticationMethod authMethod)
        {
            _ = authMethod ?? throw new ArgumentNullException(nameof(authMethod));
            using var userCert = authMethod.ClientCertificateAsX509Certificate2();

            var newUserData =
                new XElement(nsEHUC + "EapHostUserCredentials",
                    new XAttribute(XNamespace.Xmlns + "eapCommon", nsEC),
                    new XAttribute(XNamespace.Xmlns + "baseEap", nsBEMUC),
                    new XElement(nsEHUC + "EapMethod",
                        new XElement(nsEC + "Type", (int)authMethod.EapType),
                        new XElement(nsEC + "AuthorId", authMethod.EapType == EapType.TTLS ? 311 : 0)
                    //new XElement(nsEC + "AuthorId", "67532") // geant link
                    ),
                    new XElement(nsEHUC + "Credentials",
                        new XAttribute(XNamespace.Xmlns + "eapuser", nsEUP),
                        new XAttribute(XNamespace.Xmlns + "xsi", nsXSI),
                        new XAttribute(XNamespace.Xmlns + "baseEap", nsBEUP),
                        new XAttribute(XNamespace.Xmlns + "MsPeap", nsMPUP),
                        new XAttribute(XNamespace.Xmlns + "MsChapV2", nsMCUP),
                        new XAttribute(XNamespace.Xmlns + "eapTtls", nsTTLS),
                        EapUserData(
                            authMethod.ClientUserName,
                            authMethod.ClientPassword,
                            outerIdentity:
                                string.IsNullOrEmpty(authMethod.ClientOuterIdentity)
                                    ? authMethod.ClientUserName
                                    : authMethod.ClientOuterIdentity
                                    ,
                            authMethod.EapType,
                            authMethod.InnerAuthType,
                            userCert?.Thumbprint
                        )
                    )
                );

            // returns xml as string if not null
            return newUserData != null ? newUserData.ToString() : "";
        }

        private static XElement EapUserData(
            string? innerIdentity,
            string? password,
            string? outerIdentity,
            EapType eapType,
            InnerAuthType innerAuthType,
            string? userCertFingerprint = null)
        {
            return (eapType, innerAuthType) switch
            {
                (EapType.MSCHAPv2, InnerAuthType.None) =>

                    new XElement(nsBEUP + "Eap",
                        new XElement(nsBEUP + "Type", (int)EapType.MSCHAPv2),
                        new XElement(nsMCUP + "EapType",
                            new XElement(nsMCUP + "Username", innerIdentity),
                            new XElement(nsMCUP + "Password", password),
                            new XElement(nsMCUP + "LogonDomain") // TODO: what is this?
                        )
                    ),

                (EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) =>

                    new XElement(nsBEUP + "Eap",
                        new XElement(nsBEUP + "Type", (int)EapType.PEAP),
                        new XElement(nsMPUP + "EapType",
                            new XElement(nsMPUP + "RoutingIdentity", outerIdentity),
                            EapUserData(
                                innerIdentity,
                                password,
                                outerIdentity,
                                EapType.MSCHAPv2,
                                InnerAuthType.None
                            )
                        )
                    ),

                (EapType.TLS, InnerAuthType.None) =>

                    new XElement(nsBEUP + "Eap",
                        new XElement(nsBEUP + "Type", (int)EapType.TLS),
                        new XElement(nsTLS + "EapType",
                            new XElement(nsTLS + "Username", outerIdentity), // TODO: test if this gets used
                            new XElement(nsTLS + "UserCert", // xs:hexBinary
                                                             // format fingerprint:
                                Regex.Replace(Regex.Replace(userCertFingerprint ?? string.Empty, " ", ""), ".{2}", "$0 ")
                                    .ToUpperInvariant().Trim()
                            )
                        )
                    ),

                var x when
                x == (EapType.TTLS, InnerAuthType.PAP) ||
                x == (EapType.TTLS, InnerAuthType.MSCHAP) || // v1 is not tested
                x == (EapType.TTLS, InnerAuthType.MSCHAPv2) =>

                    new XElement(nsTTLS + "EapTtls", // schema says lower camelcase, but only upper camelcase works
                        new XElement(nsTTLS + "Username", innerIdentity), // outerIdentity is configured in ProfileXml
                        new XElement(nsTTLS + "Password", password)
                    ),

                (EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => // TODO: matches schema, but produces an error

                    new XElement(nsTTLS + "EapTtls",
                        //new XElement(nsTTLS + "Username", uname),
                        //new XElement(nsTTLS + "Password", pword),
                        EapUserData(
                            innerIdentity,
                            password,
                            outerIdentity,
                            EapType.MSCHAPv2,
                            InnerAuthType.None
                        )
                    ),

                (EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => // TODO: matches schema, but produces an error

                    new XElement(nsTTLS + "EapTtls",
                        //new XElement(nsTTLS + "Username", uname),
                        //new XElement(nsTTLS + "Password", pword),
                        EapUserData(
                            innerIdentity,
                            password,
                            outerIdentity,
                            EapType.PEAP,
                            InnerAuthType.EAP_MSCHAPv2
                        )
                    ),

                // not supported
                _ => throw new EduroamAppUserException("unsupported auth method"),
            };
        }

        public static bool IsSupported(AuthenticationMethod authMethod) => IsSupported(authMethod.EapType, authMethod.InnerAuthType);

        private static bool IsSupported(EapType eapType, InnerAuthType innerAuthType)
        {
            var isX86_32 = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X86;
            Debug.Assert(!isX86_32);
            //bool at_least_win10 = System.Environment.OSVersion.Version.Major >= 10; // TODO: make this work, requires some application manifest
            //var at_least_win10 = true;
            return (eapType, innerAuthType) switch
            {
                (EapType.TLS, _) => true,
                //(EapType.MSCHAPv2, InnerAuthType.None) => true,
                (EapType.PEAP, InnerAuthType.EAP_MSCHAPv2) => true,
                (EapType.TTLS, InnerAuthType.PAP) => !isX86_32,
                (EapType.TTLS, InnerAuthType.MSCHAP) => !isX86_32,
                (EapType.TTLS, InnerAuthType.MSCHAPv2) => !isX86_32,
                //(EapType.TTLS, InnerAuthType.EAP_MSCHAPv2) => at_least_win10 && !isX86, // TODO: xml matches the schema, but win32 throws an error.
                //(EapType.TTLS, InnerAuthType.EAP_PEAP_MSCHAPv2) => at_least_win10 && !isX86, // TODO: xml matches the schema, but win32 throws an error.
                _ => false,
            };
        }
    }
}
