using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    public class UnknownProfileException : ApplicationException
    {
        public UnknownProfileException(string institute, string profile) : base($"Institute '{institute}' has no profile named '{profile}'")
        {
        }

        public UnknownProfileException(string institute, string profile, Exception? innerException) : base($"Institute '{institute}' has no profile named '{profile}'", innerException)
        {
        }

        protected UnknownProfileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
