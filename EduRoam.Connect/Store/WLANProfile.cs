using System;

namespace EduRoam.Connect.Store
{
    public readonly struct WLANProfile
    {
        public Guid InterfaceId { get; }
        public string ProfileName { get; }
        public bool IsHs2 { get; }
        public bool HasUserData { get; }

        public WLANProfile(Guid interfaceId, string profileName, bool isHs2, bool hasUserData = false)
        {
            this.InterfaceId = interfaceId;
            this.ProfileName = profileName;
            this.IsHs2 = isHs2;
            this.HasUserData = hasUserData;
        }

        public WLANProfile WithUserDataSet()
            => new(
                interfaceId: this.InterfaceId,
                profileName: this.ProfileName,
                isHs2: this.IsHs2,
                hasUserData: true);
    }

}
