using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
	public class InternetConnectionException : Exception
	{
		private const string DefaultMessage = "Error with internet connection";

		public InternetConnectionException() : base(DefaultMessage) { }

		public InternetConnectionException(string message) : base(message) { }

		public InternetConnectionException(string message, Exception innerException) : base(message, innerException) { }

		protected InternetConnectionException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}

