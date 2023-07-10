using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
	public class EduroamAppUserException : Exception
	{
		public string UserFacingMessage { get; }

		public EduroamAppUserException(string message, string userFacingMessage = null) : base(message)
		{
#if DEBUG
			UserFacingMessage = userFacingMessage ?? ("NON-USER-FACING-MESSAGE: " + message);
#else
			UserFacingMessage = userFacingMessage ?? "NO REASON PROVIDED"; // TODO: rethink this strategy...
#endif
		}

		protected EduroamAppUserException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}

