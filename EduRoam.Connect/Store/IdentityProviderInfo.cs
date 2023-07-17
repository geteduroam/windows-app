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
        public string EapConfigXml { get; } // optional, used for reinstall of userprofile
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
            string eapConfigXml)
        {
            DisplayName = displayName;
            EmailAddress = emailAddress;
            WebAddress = webAddress;
            Phone = phone;
            InstId = instId;
            ProfileId = profileId;
            IsOauth = isOauth;
            NotBefore = notBefore;
            NotAfter = notAfter;
            EapTypeSsid = eapTypeSsid;
            EapTypeHs2 = eapTypeHs2;
            EapConfigXml = eapConfigXml;
        }

        public static IdentityProviderInfo From(EapConfig.AuthenticationMethod authMethod)
            => authMethod == null
                ? throw new ArgumentNullException(paramName: nameof(authMethod))
                : new IdentityProviderInfo(
                    authMethod.EapConfig.InstitutionInfo.DisplayName,
                    authMethod.EapConfig.InstitutionInfo.EmailAddress,
                    authMethod.EapConfig.InstitutionInfo.WebAddress,
                    authMethod.EapConfig.InstitutionInfo.Phone,
                    authMethod.EapConfig.InstitutionInfo.InstId,
                    authMethod.EapConfig.ProfileId,
                    authMethod.EapConfig.IsOauth,
                    authMethod.ClientCertificateNotBefore,
                    authMethod.ClientCertificateNotAfter,
                    eapTypeSsid: authMethod.IsSSIDSupported
                        ? (authMethod.EapType, authMethod.InnerAuthType)
                        : ((EapType, InnerAuthType)?)null,
                    eapTypeHs2: authMethod.IsHS20Supported
                        ? (authMethod.EapType, authMethod.InnerAuthType)
                        : ((EapType, InnerAuthType)?)null,
                    eapConfigXml:
                        authMethod.EapType != EapType.TLS
                        && string.IsNullOrEmpty(authMethod.ClientCertificate) // should never happen with CAT nor letswifi
                        && string.IsNullOrEmpty(authMethod.ClientPassword)
                        && !authMethod.EapConfig.IsOauth
                            ? authMethod.EapConfig.RawOriginalEapConfigXmlData
                            : null);

        public IdentityProviderInfo WithEapConfigXml(string eapConfigXml)
            => new IdentityProviderInfo(DisplayName, EmailAddress, WebAddress,
                Phone, InstId, ProfileId, IsOauth, NotBefore, NotAfter,
                EapTypeSsid, EapTypeHs2, eapConfigXml);
    }

}
