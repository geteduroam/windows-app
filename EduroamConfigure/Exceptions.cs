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

	public class ApiUnreachableException : Exception
	{
		private static readonly string DefaultMessage = "Api could not be reached";

		public ApiUnreachableException() : base(DefaultMessage) { }
		public ApiUnreachableException(string message) : base(message) { }
		public ApiUnreachableException(string message, System.Exception innerException) : base(message, innerException) { }
	}

	public class ApiParsingException : Exception
	{
		private static readonly string DefaultMessage = "Api response could not be parsed";

		public ApiParsingException() : base(DefaultMessage) { }
		public ApiParsingException(string message) : base(message) { }
		public ApiParsingException(string message, System.Exception innerException) : base(message, innerException) { }
	}
}

