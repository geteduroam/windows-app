using System.Collections.Generic;

using EduRoam.Connect.Identity;

namespace EduRoam.Connect
{
    public static class Cache
    {
        public static List<IdentityProvider>? IdentityProviders { get; set; } = null;
    }
}