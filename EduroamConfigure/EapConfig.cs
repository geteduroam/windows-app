using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace EduroamConfigure
{
	/// <summary>
	/// Stores information found in an EAP-config file.
	/// </summary>
	public class EapConfig
	{
		#region Properties

		public bool IsOauth { get; set; } // TODO: Setter used for scaffolding to PersistenStorage, need better solution
		public string ProfileId { get; set; } // TODO: Setter used for scaffolding to PersistenStorage, need better solution
		public List<AuthenticationMethod> AuthenticationMethods { get; }
		public List<CredentialApplicability> CredentialApplicabilities { get; }
		public ProviderInfo InstitutionInfo { get; }
		public string XmlData { get; }

		#endregion

		#region Helpers

		public IEnumerable<string> SSIDs { get => CredentialApplicabilities.Select((c) => c.Ssid); }
		public IEnumerable<string> ConsortiumOids { get => CredentialApplicabilities.Select((c) => c.ConsortiumOid); }

		#endregion

		#region Constructor

		private EapConfig(
			List<AuthenticationMethod> authenticationMethods,
			List<CredentialApplicability> credentialApplicabilities,
			ProviderInfo institutionInfo,
			string xmlData)
		{
			AuthenticationMethods = authenticationMethods;
			CredentialApplicabilities = credentialApplicabilities;
			InstitutionInfo = institutionInfo;
			XmlData = xmlData;

			AuthenticationMethods.ForEach(authMethod =>
			{
				authMethod.EapConfig = this;
			});
		}

		#endregion


		/// <summary>
		/// AuthenticationMethod contains information about client certificates and CAs.
		/// </summary>
		public class AuthenticationMethod
		{
			#region Properties

			public EapConfig EapConfig { get; set; } // reference to parent EapConfig
			public EapType EapType { get; }
			public InnerAuthType InnerAuthType { get; }
			public List<string> ServerCertificateAuthorities { get; } // base64 encoded DER certificate
			public List<string> ServerNames { get; }
			public string ClientUserName { get; } // preset inner identity, expect it to have a realm
			public string ClientPassword { get; } // preset outer identity
			public string ClientCertificate { get; } // base64 encoded PKCS12 certificate+privkey bundle
			public string ClientCertificatePassphrase { get; } // passphrase for ^
			public string ClientOuterIdentity { get; } // expect it to have a realm. Also known as: anonymous identity, routing identity
			public string ClientInnerIdentitySuffix { get; } // realm
			public bool ClientInnerIdentityHint { get; } // Wether to disallow subrealms or not (see https://github.com/GEANT/CAT/issues/190)

			public bool IsHS20Supported { get => EapConfig.CredentialApplicabilities.Any(cred => cred.ConsortiumOid != null); }
			public bool IsSSIDSupported { get => EapConfig.CredentialApplicabilities.Any(cred => cred.Ssid != null && cred.Ssid.Length != 0); }

			#endregion Properties
			#region Helpers

			// TODO: Also add wired 802.1x support
			public List<string> SSIDs
			{
				get => EapConfig.CredentialApplicabilities
					.Where(cred => cred.NetworkType == IEEE802x.IEEE80211)
					.Where(cred => cred.MinRsnProto != "TKIP") // Too old and insecure
					.Where(cred => cred.Ssid != null) // Filter out HS20 entries, those have no SSID
					.Select(cred => cred.Ssid)
					.ToList();
			}
			public List<string> ConsortiumOIDs
			{
				get => EapConfig.CredentialApplicabilities
					.Where(cred => cred.ConsortiumOid != null)
					.Select(cred => cred.ConsortiumOid)
					.ToList();
			}
			public DateTime? ClientCertificateNotBefore
			{
				get
				{
					using var cert = ClientCertificateAsX509Certificate2();
					return cert?.NotBefore;
				}
			}
			public DateTime? ClientCertificateNotAfter
			{
				get
				{
					using var cert = ClientCertificateAsX509Certificate2();
					return cert?.NotAfter;
				}
			}


			private byte[] ClientCertificateRaw
			{ get => Convert.FromBase64String(ClientCertificate); }
			private bool CertificateIsValid
			{
				get => VerifyCertificateBundle(
						Convert.FromBase64String(ClientCertificate),
						ClientCertificatePassphrase);
			}

			#endregion Helpers
			#region CertExport

			/// <summary>
			/// Converts and enumerates CertificateAuthorities as X509Certificate2 objects.
			/// </summary>
			/// <remarks>The certificates must be disposed after use</remarks>
			public IEnumerable<X509Certificate2> CertificateAuthoritiesAsX509Certificate2()
			{
				foreach (var ca in ServerCertificateAuthorities)
				{
					X509Certificate2 cert;
					try
					{
						// TODO: find some nice way to ensure these are disposed of properly
						cert = new X509Certificate2(Convert.FromBase64String(ca));
					}
					catch (CryptographicException)
					{
						throw new EduroamAppUserException("corrupt certificate",
							"EAP profile has an malformed or corrupted certificate");
					}

					// sets the friendly name of certificate
					if (string.IsNullOrEmpty(cert.FriendlyName))
						cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, false);

					yield return cert;
				}
			}

			/// <summary>
			/// Converts the client certificate base64 data to a X509Certificate2 object
			/// </summary>
			/// <returns>X509Certificate2 if any. Null if non exist or the passphrase is incorrect</returns>
			public X509Certificate2 ClientCertificateAsX509Certificate2()
			{
				if (string.IsNullOrEmpty(ClientCertificate))
					return null;

				try
				{
					var cert = new X509Certificate2(
						Convert.FromBase64String(ClientCertificate),
						ClientCertificatePassphrase,
						X509KeyStorageFlags.PersistKeySet);

					// sets the friendly name of certificate
					if (string.IsNullOrEmpty(cert.FriendlyName))
						cert.FriendlyName = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);

					return cert;
				}
				catch (CryptographicException ex)
				{
					if ((ex.HResult & 0xFFFF) == 0x56)
						return null; // wrong passphrase

					throw new EduroamAppUserException("corrupt client certificate",
						"EAP profile has an malformed or corrupted client certificate");
				}
			}

			#endregion CertExport
			#region Verification

			// methods to check if the authentification method is complete, and methods to mend it

			/// <summary>
			/// If this returns true, then the user must provide the login credentials
			/// when installing with ConnectToEduroam or EduroamNetwork
			/// </summary>
			public bool NeedsLoginCredentials
			{
				get => EapType != EapType.TLS // If the auth method expects login credentials
					&& string.IsNullOrEmpty(ClientUserName) // but we don't already have them
					|| string.IsNullOrEmpty(ClientPassword);
			}

			/// <summary>
			/// If this is true, then you must provide a
			/// certificate file and add it with this.AddClientCertificate
			/// </summary>
			public bool NeedsClientCertificate
			{
				get => EapType == EapType.TLS // If we use EAP-TLS
					&& string.IsNullOrEmpty(ClientCertificate); // and we don't already have a certificate
			}

			/// <summary>
			/// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
			/// Add this passphrase with this.AddClientCertificatePassphrase
			/// </summary>
			public bool NeedsClientCertificatePassphrase
			{
				get => EapType == EapType.TLS // If we use EAP-TLS
					&& !string.IsNullOrEmpty(ClientCertificate) // and we DO have a client certificate
					&& !CertificateIsValid; // but we cannot read it yet
			}

			#endregion Verification

			#region ClientCertificate
			/// <summary>
			/// Adds the username and password to be installed along with the wlan profile
			/// </summary>
			/// <param name="username">The username for inner auth</param>
			/// <param name="password">The passpword for inner auth</param>
			/// <returns>Clone of this object with the appropriate properties set</returns>
			public AuthenticationMethod WithLoginCredentials(string username, string password)
				=> new AuthenticationMethod(
					EapType,
					InnerAuthType,
					ServerCertificateAuthorities,
					ServerNames,
					username,
					password,
					ClientCertificate,
					ClientCertificatePassphrase,
					ClientOuterIdentity,
					ClientInnerIdentitySuffix,
					ClientInnerIdentityHint);

			/// <summary>
			/// Reads and adds the user certificate to be installed along with the wlan profile
			/// </summary>
			/// <param name="filePath">path to the certificate file in question. PKCS12</param>
			/// <param name="passphrase">the passphrase to the certificate file in question</param>
			/// <returns>Clone of this object with the appropriate properties set</returns>
			public AuthenticationMethod WithClientCertificate(string filePath, string passphrase = null)
				=> VerifyCertificateBundle(filePath, passphrase)
					? new AuthenticationMethod(
						EapType,
						InnerAuthType,
						ServerCertificateAuthorities,
						ServerNames,
						ClientUserName,
						ClientPassword,
						Convert.ToBase64String(File.ReadAllBytes(filePath)),
						passphrase,
						ClientOuterIdentity,
						ClientInnerIdentitySuffix,
						ClientInnerIdentityHint)
					: null
					;

			/// <summary>
			/// Sets the passphrase to use when derypting the certificate bundle.
			/// Will only be stored if valid.
			/// </summary>
			/// <param name="passphrase">the passphrase to the certificate</param>
			/// <returns>Clone of this object with the appropriate properties set</returns>
			public AuthenticationMethod WithClientCertificatePassphrase(string passphrase)
				=> VerifyCertificateBundle(ClientCertificateRaw, passphrase)
					? new AuthenticationMethod(
						EapType,
						InnerAuthType,
						ServerCertificateAuthorities,
						ServerNames,
						ClientUserName,
						ClientPassword,
						ClientCertificate,
						passphrase,
						ClientOuterIdentity,
						ClientInnerIdentitySuffix,
						ClientInnerIdentityHint)
					: null
					;

			/// <summary>
			/// Helper function which verifies if the
			/// certificate data and the passphrase is as valid combo
			/// </summary>
			/// <param name="rawCertificateData">Certificate data, PKCS12</param>
			/// <param name="passphrase">the passphrase to the certificate file in question</param>
			/// <returns>true if valid</returns>
			private static bool VerifyCertificateBundle(byte[] rawCertificateData, string passphrase = null)
			{
				try
				{
					using var testCertificate = new X509Certificate2(rawCertificateData, passphrase);
				}
				catch (CryptographicException ex)
				{
					if ((ex.HResult & 0xFFFF) == 0x56) return false; // wrong passphrase

					Debug.WriteLine("THIS SHOULD NOT HAPPEN");
					Debug.Print(ex.ToString());
					Debug.Assert(false);
					throw;
				}
				return true;
			}

			/// <summary>
			/// Helper function which verifies if the filepath exists and
			/// that the passphrase is valid to read the certificate bundle
			/// </summary>
			/// <param name="filePath">path to the certificate file in question. PKCS12</param>
			/// <param name="passphrase">the passphrase to the certificate file in question</param>
			/// <returns>true if valid</returns>
			private static bool VerifyCertificateBundle(string filePath, string passphrase = null)
			{
				if (filePath == null)
					throw new ArgumentNullException(paramName: nameof(filePath));
				if (!File.Exists(filePath))
					throw new ArgumentException(paramName: nameof(filePath), message: "file not found");

				return VerifyCertificateBundle(File.ReadAllBytes(filePath), passphrase);
			}

			#endregion ClientCertificate

			// Constructor
			public AuthenticationMethod(
				EapType eapType,
				InnerAuthType innerAuthType,
				List<string> serverCertificateAuthorities,
				List<string> serverName,
				string clientUserName = null,
				string clientPassword = null,
				string clientCertificate = null,
				string clientCertificatePassphrase = null,
				string clientOuterIdentity = null,
				string innerIdentitySuffix = null,
				bool innerIdentityHint = false
			)
			{
				EapType = eapType;
				InnerAuthType = innerAuthType;
				ServerCertificateAuthorities = serverCertificateAuthorities ?? new List<string>();
				ServerNames = serverName ?? new List<string>();
				ClientUserName = clientUserName;
				ClientPassword = clientPassword;
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
		public readonly struct ProviderInfo
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
			public (double Latitude, double Longitude)? Location { get; } // nullable coordinates on the form (Latitude, Longitude)

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
				ValueTuple<double, double>? location)
			{
				DisplayName = displayName;
				Description = description;
				LogoData = logoData;
				LogoMimeType = logoMimeType;
				EmailAddress = emailAddress;
				WebAddress = webAddress;
				Phone = phone;
				InstId = instId;
				TermsOfUse = termsOfUse;
				Location = location;
			}
		}

		/// <summary>
		/// Container for an entry in the 'CredentialApplicabilitis' section in the EAP config xml.
		/// Each entry denotes a way to configure this EAP config.
		/// There are threedifferent cases:
		/// - WPA with SSID:
		///         NetworkType == IEEE80211 and Ssid != null
		/// - WPA with Hotspot2.0:
		///         NetworkType == IEEE80211 and ConsortiumOid != null
		/// - Wired 801x:
		///         NetworkType == IEEE80211 and NetworkId != null
		/// </summary>
		public readonly struct CredentialApplicability
		{
			public IEEE802x NetworkType { get; }

			// IEEE80211 only:

			/// <summary>
			/// NetworkType == IEEE80211 only. Used to configure WPA with SSID
			/// </summary>
			public string Ssid { get; } // Wifi SSID, TODO: use
			/// <summary>
			/// NetworkType == IEEE80211 only. Used to configure WPA with Hotspot 2.0
			/// </summary>
			public string ConsortiumOid { get; } // Hotspot2.0
			/// <summary>
			/// NetworkType == IEEE80211 only, Has either a value of "TKIP" or "CCMP"
			/// </summary>
			public string MinRsnProto { get; } // "TKIP" or "CCMP"


			// IEEE8023 only:

			/// <summary>
			/// NetworkType == IEEE8023 only
			/// </summary>
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
		/// <exception cref="XmlException">Parsing <paramref name="eapConfigXmlData"/> failed</exception>
		public static EapConfig FromXmlData(string eapConfigXmlData)
		{
			// XML format Documentation:
			// Current:  https://github.com/GEANT/CAT/blob/master/devices/eap_config/eap-metadata.xsd
			// Outdated: https://tools.ietf.org/id/draft-winter-opsawg-eap-metadata-00.html

			// TODO: validate the file first. use schema?
			// TODO: add a test on this function using fuzzing accoring to schema

			static Func<XElement, bool> nameIs(string name) => // shorthand lambda
				element => element.Name.LocalName == name;

			// load the XML file into a XElement object
			XElement eapConfigXml;
			try
			{
				eapConfigXml = XElement.Parse(eapConfigXmlData);
			}
			catch (XmlException)
			{
				throw; // explicitly show that XmlException can be thrown here
			}
			/*
			foreach (XElement eapIdentityProvider in eapConfigXml.Descendants().Where(nameIs("EAPIdentityProvider")))
			{
				// NICE TO HAVE: yield return from this
			}
			*/

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
				EapType eapType = (EapType)(int)authMethodXml
					.Elements().First(nameIs("EAPMethod"))
					.Elements().First(nameIs("Type"));

				InnerAuthType innerAuthType = (InnerAuthType?)(int?)authMethodXml
					.Elements().FirstOrDefault(nameIs("InnerAuthenticationMethod"))
					?.Descendants().FirstOrDefault(nameIs("Type"))
					?? InnerAuthType.None;

				// ServerSideCredential

				// get list of strings of CA certificates
				List<string> serverCAs = serverSideCredentialXml
					.Elements().Where(nameIs("CA")) // TODO: <CA format="X.509" encoding="base64"> is assumed, schema does not enforce this
					.Select(xElement => (string)xElement)
					.ToList();

				// get list of strings of server IDs
				List<string> serverNames = serverSideCredentialXml
					.Elements().Where(nameIs("ServerID"))
					.Select(xElement => (string)xElement)
					.ToList();

				// ClientSideCredential

				// Preset credentials
				var clientUserName = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("UserName"));
				var clientPassword = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("Password"));
				var clientCert = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("ClientCertificate")); // TODO: <ClientCertificate format="PKCS12" encoding="base64"> is assumed
				var clientCertPasswd = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("Passphrase"));

				// inner/outer identity
				var clientOuterIdentity = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("OuterIdentity"));
				var clientInnerIdentitySuffix = (string)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("InnerIdentitySuffix"));
				var clientInnerIdentityHint = (bool?)clientSideCredentialXml
					?.Elements().FirstOrDefault(nameIs("InnerIdentityHint")) ?? false;

				// create new authentication method object and adds it to list
				authMethods.Add(new EapConfig.AuthenticationMethod(
					eapType,
					innerAuthType,
					serverCAs,
					serverNames,
					clientUserName,
					clientPassword,
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
				.Descendants().FirstOrDefault(nameIs("EAPIdentityProvider")); // NICE TO HAVE: update this if the yield return above gets used

			// get institution ID from identity element
			var instId = (string)eapIdentityElement.Attribute("ID");

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
					location),
				eapConfigXmlData
			);
		}

		/// <summary>
		/// Yields EapAuthMethodInstallers which will attempt to install eapConfig for you.
		/// Refer to frmSummary.InstallEapConfig to see how to use it (TODO: actually explain when finalized)
		/// </summary>
		/// <param name="eapConfig">EapConfig object</param>
		/// <returns>Enumeration of EapAuthMethodInstaller intances for each supported authentification method in eapConfig</returns>
		public IEnumerable<AuthenticationMethod> SupportedAuthenticationMethods
		{
			get => AuthenticationMethods.Where(EduroamNetwork.IsAuthMethodSupported);
		}

		/// <summary>
		/// If this returns true, then the user must provide the login credentials
		/// when installing with ConnectToEduroam or EduroamNetwork
		/// </summary>
		public bool NeedsLoginCredentials
		{
			get => AuthenticationMethods.Any(authMethod => authMethod.NeedsLoginCredentials);
		}

		/// <summary>
		/// If this is true, then you must provide a
		/// certificate file and add it with this.AddClientCertificate
		/// </summary>
		public bool NeedsClientCertificate
		{
			get => AuthenticationMethods
				.Any(authMethod => authMethod.NeedsClientCertificate);
		}

		/// <summary>
		/// If this is true, then the user must provide a passphrase to the bundled certificate bundle.
		/// Add this passphrase with this.AddClientCertificatePassphrase
		/// </summary>
		public bool NeedsClientCertificatePassphrase
		{
			get => AuthenticationMethods
				.Any(authMethod => authMethod.NeedsClientCertificatePassphrase);
		}

		/// <summary>
		/// Reads and adds the user certificate to be installed along with the wlan profile
		/// </summary>
		/// <param name="filePath">path to the certificate file in question. PKCS12</param>
		/// <param name="passphrase">the passphrase to the certificate file in question</param>
		/// <returns>Clone of this object with the appropriate properties set</returns>
		/// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
		public EapConfig WithClientCertificate(string certificatePath, string certificatePassphrase = null)
		{
			IEnumerable<AuthenticationMethod> authMethods = AuthenticationMethods.Select(authMethod => authMethod.WithClientCertificate(certificatePath, certificatePassphrase)).Where(x => x != null);
			if (!authMethods.Any()) throw new ArgumentException("No authentication method can accept the client certificate");

			return new EapConfig(
				authMethods.ToList(),
				CredentialApplicabilities,
				InstitutionInfo,
				XmlData
			);
		}

		/// <summary>
		/// Sets the passphrase to use when derypting the certificate bundle.
		/// Will only be stored if valid.
		/// </summary>
		/// <param name="passphrase">the passphrase to the certificate</param>
		/// <returns>Clone of this object with the appropriate properties set</returns>
		/// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
		public EapConfig WithClientCertificatePassphrase(string certificatePassphrase)
		{
			IEnumerable<AuthenticationMethod> authMethods = AuthenticationMethods.Select(authMethod => authMethod.WithClientCertificatePassphrase(certificatePassphrase)).Where(x => x != null);
			if (!authMethods.Any()) throw new ArgumentException("No authentication accepts the passphrase");

			return new EapConfig(
				authMethods.ToList(),
				CredentialApplicabilities,
				InstitutionInfo,
				XmlData
			);
		}

		/// <summary>
		/// Sets the username/password for inner auth.
		/// </summary>
		/// <param name="username">The username for inner auth</param>
		/// <param name="password">The passpword for inner auth</param>
		/// <returns>Clone of this object with the appropriate properties set</returns>
		/// <exception cref="ArgumentException">The client certificate was not accepted by any authentication method</exception>
		public EapConfig WithLoginCredentials(string username, string password)
		{
			IEnumerable<AuthenticationMethod> authMethods = AuthenticationMethods.Select(authMethod => authMethod.WithLoginCredentials(username, password)).Where(x => x != null);
			if (!authMethods.Any()) throw new ArgumentException("No authentication accepts the passphrase");

			return new EapConfig(
				authMethods.ToList(),
				CredentialApplicabilities,
				InstitutionInfo,
				XmlData
			);
		}

		/// <summary>
		/// goes through all the AuthenticationMethods in this config and tries to reason about a correct method
		/// the suffix may be null or empty, null means no realm check must be done,
		/// empty means that any realm is valid but a realm must be provided
		/// https://github.com/GEANT/CAT/blob/master/tutorials/MappingCATOptionsIntoSupplicantConfig.md#verify-user-input-to-contain-realm-suffix-checkbox
		/// </summary>
		/// <returns>A ValueTuple with the inner identity suffix and hint</returns>
		public (string suffix, bool hint) GetClientInnerIdentityRestrictions()
		{
			var hint = AuthenticationMethods
				.All(authMethod => authMethod.ClientInnerIdentityHint);
			var suffi = AuthenticationMethods
				.Select(authMethod => authMethod.ClientInnerIdentitySuffix)
				.ToList();

			string suffix = null;
			if (suffi.Any())
			{
				var first = suffi.First();
				if (suffi.All(suffix => suffix == first))
					suffix = first;
			}
			return (suffix, hint);
		}

	}

	public enum IEEE802x
	{
		/// <summary>
		/// Wired LAN
		/// </summary>
		IEEE8023, // TODO: add full support for this (wired x802)

		/// <summary>
		/// Wireless LAN
		/// </summary>
		IEEE80211
	}

	/// <summary>
	///  https://www.vocal.com/secure-communication/eap-types/
	/// </summary>
	public enum EapType
	{
		TLS = 13,
		TTLS = 21,
		PEAP = 25,
		MSCHAPv2 = 26,
	}

	/// <summary>
	/// The type of authentification used in the inner tunnel.
	/// Also known as stage 2 authentification.
	/// </summary>
	public enum InnerAuthType
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
