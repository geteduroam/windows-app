using EduRoam.Connect.Eap;
using EduRoam.Connect.Language;

namespace EduRoam.Connect.Store
{
    public readonly struct IdentityProviderInfo
    {
        public string DisplayName { get; }
        public string EmailAddress { get; }
        public string WebAddress { get; }
        public string Phone { get; }
        public string InstId { get; }
        public string? ProfileId { get; }
        public bool IsOauth { get; }
        public DateTime? NotBefore { get; }
        public DateTime? NotAfter { get; }
        public (EapType outer, InnerAuthType inner)? EapTypeSsid { get; }
        public (EapType outer, InnerAuthType inner)? EapTypeHs2 { get; }
        public string? EapConfigXml { get; } // optional, used for reinstall of userprofile
                                             // TODO: registry might have a max-size limit. ^ include institution image

        public IdentityProviderInfo(
            string displayName,
            string emailAddress,
            string        /* how are you? */     webAddress,
            string phone,
            string instId,
            string profileId,
            bool isOauth,
            DateTime? notBefore,
            DateTime? notAfter,
            (EapType outer, InnerAuthType inner)? eapTypeSsid,
            (EapType outer, InnerAuthType inner)? eapTypeHs2,
            string? eapConfigXml)
        {
            this.DisplayName = displayName;
            this.EmailAddress = emailAddress;
            this.WebAddress = webAddress;
            this.Phone = phone;
            this.InstId = instId;
            this.ProfileId = profileId;
            this.IsOauth = isOauth;
            this.NotBefore = notBefore;
            this.NotAfter = notAfter;
            this.EapTypeSsid = eapTypeSsid;
            this.EapTypeHs2 = eapTypeHs2;
            this.EapConfigXml = eapConfigXml;
        }

        public static IdentityProviderInfo From(Eap.AuthenticationMethod? authMethod)
        {
            if (authMethod == null)
            {
                throw new ArgumentNullException(paramName: nameof(authMethod));
            }

            var eapConfig = authMethod.EapConfig;

            return eapConfig == null
                ? throw new ArgumentException(Resources.ErrorEapConfigIsEmpty)
                : new IdentityProviderInfo(
                   eapConfig.InstitutionInfo.DisplayName,
                   eapConfig.InstitutionInfo.EmailAddress,
                   eapConfig.InstitutionInfo.WebAddress,
                   eapConfig.InstitutionInfo.Phone,
                   eapConfig.InstitutionInfo.InstId,
                   eapConfig.ProfileId,
                   eapConfig.IsOauth,
                   authMethod.ClientCertificateNotBefore,
                   authMethod.ClientCertificateNotAfter,
                   eapTypeSsid: authMethod.IsSSIDSupported ? (authMethod.EapType, authMethod.InnerAuthType) : ((EapType, InnerAuthType)?)null,
                   eapTypeHs2: authMethod.IsHS20Supported ? (authMethod.EapType, authMethod.InnerAuthType) : ((EapType, InnerAuthType)?)null,
                   eapConfigXml:
                       authMethod.EapType != EapType.TLS
                       && string.IsNullOrEmpty(authMethod.ClientCertificate) // should never happen with CAT nor letswifi
                       && string.IsNullOrEmpty(authMethod.ClientPassword)
                       && !eapConfig.IsOauth
                           ? eapConfig.RawOriginalEapConfigXmlData
                           : null);
        }

        public IdentityProviderInfo WithEapConfigXml(string eapConfigXml)
            => new(this.DisplayName, this.EmailAddress, this.WebAddress,
                this.Phone, this.InstId, this.ProfileId, this.IsOauth, this.NotBefore, this.NotAfter,
                this.EapTypeSsid, this.EapTypeHs2, eapConfigXml);
    }

}
