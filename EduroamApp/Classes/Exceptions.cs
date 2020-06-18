using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    class EduroamAppUserError : Exception
    {
        public string UserFacingMessage { get; }

        public EduroamAppUserError(string message, string userFacingMessage = "NO REASON PROVIDED") : base(message)
        {
            UserFacingMessage = userFacingMessage;
        }
    }
}
