using System;

namespace EduRoam.Connect.Eap
{

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
            this.DisplayName = displayName;
            this.Description = description;
            this.LogoData = logoData;
            this.LogoMimeType = logoMimeType;
            this.EmailAddress = emailAddress;
            this.WebAddress = webAddress;
            this.Phone = phone;
            this.InstId = instId;
            this.TermsOfUse = termsOfUse;
            this.Location = location;
        }
    }
}
