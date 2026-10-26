using System;
using System.Runtime.Serialization;

namespace EduRoam.Connect.Exceptions
{
    [Serializable]
    public class EduroamAppUserException : Exception
    {
        public string UserFacingMessage { get; }

        public EduroamAppUserException(string message, string? userFacingMessage = null) : base(message)
        {
#if DEBUG
            this.UserFacingMessage = userFacingMessage ?? ("NON-USER-FACING-MESSAGE: " + message);
#else
            this.UserFacingMessage = userFacingMessage ?? "NO REASON PROVIDED"; // TODO: rethink this strategy...
#endif
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
            info.AddValue(nameof(this.UserFacingMessage), this.UserFacingMessage);
        }

        protected EduroamAppUserException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            var userFacingMessage = serializationInfo.GetString(nameof(this.UserFacingMessage));
            if (string.IsNullOrWhiteSpace(userFacingMessage))
            {
                throw new SerializationException($"Missing or invalid {nameof(this.UserFacingMessage)} value.");
            }
            this.UserFacingMessage = userFacingMessage;
        }
    }
}

