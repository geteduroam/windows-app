namespace EduRoam.Connect.Eap
{

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
        public string? Ssid { get; } // Wifi SSID, TODO: use
        /// <summary>
        /// NetworkType == IEEE80211 only. Used to configure WPA with Hotspot 2.0
        /// </summary>
        public string? ConsortiumOid { get; } // Hotspot2.0
        /// <summary>
        /// NetworkType == IEEE80211 only, Has either a value of "TKIP" or "CCMP"
        /// </summary>
        public string? MinRsnProto { get; } // "TKIP" or "CCMP"

        // IEEE8023 only:

        /// <summary>
        /// NetworkType == IEEE8023 only
        /// </summary>
        public string? NetworkId { get; }

        private CredentialApplicability(
            IEEE802x networkType,
            string? ssid,
            string? consortiumOid,
            string? minRsnProto,
            string? networkId)
        {

            this.NetworkType = networkType;
            this.Ssid = ssid;
            this.ConsortiumOid = consortiumOid;
            this.MinRsnProto = minRsnProto;
            this.NetworkId = networkId;
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
}
