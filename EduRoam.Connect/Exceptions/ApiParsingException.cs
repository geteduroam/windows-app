using System;
using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{

    [Serializable]
    public class ApiParsingException : Exception
    {
        private const string DefaultMessage = "Api response could not be parsed";

        public ApiParsingException() : base(DefaultMessage) { }

        public ApiParsingException(string message) : base(message) { }

        public ApiParsingException(string message, Exception innerException) : base(message, innerException) { }

        protected ApiParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

