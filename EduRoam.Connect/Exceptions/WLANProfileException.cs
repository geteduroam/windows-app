using System;
using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
    public class WLANProfileException : Exception
    {
        public WLANProfileException(string message) : base(message) { }

        public WLANProfileException(string message, Exception innerException) : base(message, innerException) { }

        protected WLANProfileException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        public WLANProfileException() { }
    }
}

