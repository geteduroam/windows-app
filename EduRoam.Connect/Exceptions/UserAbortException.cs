using System;
using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
    public class UserAbortException : Exception
    {
        public UserAbortException(string message) : base(message) { }

        public UserAbortException(string message, Exception innerException) : base(message, innerException) { }

        protected UserAbortException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        public UserAbortException() { }
    }
}

