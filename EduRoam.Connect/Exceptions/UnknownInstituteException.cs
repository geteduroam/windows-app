using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    public class UnknownInstituteException : ApplicationException
    {
        public UnknownInstituteException(string institute) : base($"Uknown institute '{institute}'")
        {
        }

        public UnknownInstituteException(string institute, Exception? innerException) : base($"Uknown institute '{institute}'", innerException)
        {
        }

        protected UnknownInstituteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
