using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamConfigure
{
    public class EduroamAppUserError : Exception
    {
        public string UserFacingMessage { get; }

        public EduroamAppUserError(string message, string userFacingMessage = null) : base(message)
        {
#if DEBUG
            UserFacingMessage = userFacingMessage ?? ("NON-USER-FACING-MESSAGE: " + message);
#else
            UserFacingMessage = userFacingMessage ?? "NO REASON PROVIDED";
#endif
        }
    }
}
