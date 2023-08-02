namespace EduRoam.Connect
{
    public enum RefreshResponse
    {
        /// <summary>
        /// All is good chief
        /// </summary>
        Success,

        /// <summary>
        /// If the profile was not refreshibly through LetsWifi, but
        /// instead managed to update the profile for a Reinstall
        /// </summary>
        UpdatedEapXml,

        /// <summary>
        /// The user has to install some new root certificates.
        /// User intevention is required
        /// </summary>
        NewRootCaRequired,

        /// <summary>
        /// The refresh token was denied. the user has to reauthenticate
        /// </summary>
        AccessDenied,

        /// <summary>
        /// There is no need to refresh the EAP profile, since it is still valid for quite some time
        /// </summary>
        StillValid,

        /// <summary>
        /// The installed profile is not refreshable
        /// </summary>
        NotRefreshable,

        /// <summary>
        /// Something failed
        /// </summary>
        Failed,
    }
}
