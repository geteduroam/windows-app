namespace EduRoam.Connect.Eap
{
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
