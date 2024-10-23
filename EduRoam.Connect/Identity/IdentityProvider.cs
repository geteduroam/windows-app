// GeoCoordinate => Coordinate
using System.Collections.Generic;

namespace EduRoam.Connect.Identity
{

    // Stores information found in IdentityProvider json.
    public class IdentityProvider
    {
        public string Country { get; set; } // ISO2
        public string Name { get; set; }
        public string Id { get; set; }
        public List<IdentityProviderProfile> Profiles { get; set; }

        public List<string> SearchTags { get; set; } = new List<string>();

        /// <summary>
        /// How the institution is shown to the end user
        /// </summary>
        /// <returns>Name of institution</returns>
        public override string ToString()
             => this.Name;

    }
}
