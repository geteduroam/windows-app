using EduRoam.Connect.Exceptions;

using System;
using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
	public class ApiUnreachableException : ApiException
	{
		private const string DefaultMessage = "Api could not be reached";

		public ApiUnreachableException() : base(DefaultMessage) { }

		public ApiUnreachableException(string message) : base(message) { }

		public ApiUnreachableException(string message, Exception innerException) : base(message, innerException) { }

		protected ApiUnreachableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

