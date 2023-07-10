using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
    public class ApiException : Exception
    {
        private const string DefaultMessage = "Error with Api";

        public ApiException() : base(DefaultMessage) { }

        public ApiException(string message) : base(message) { }

        public ApiException(string message, Exception innerException) : base(message, innerException) { }

        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}

