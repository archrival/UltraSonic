using System;
using System.Runtime.Serialization;

namespace Subsonic.Rest.Api
{
    [Serializable]
    public class SubsonicErrorException : Exception
    {
        public SubsonicErrorException(string message, Error error) : base(message)
        {
            Error = error;
        }

        public SubsonicErrorException(string message) : base(message)
        {
            Error = new Error {Message = message, Code = -1};
        }

        public SubsonicErrorException(string message, Exception innerException) : base(message, innerException)
        {
            Error = new Error {Message = message, Code = -1};
        }

        protected SubsonicErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private Error Error { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("error", Error.Message);
        }
    }

    [Serializable]
    public class SubsonicApiException : Exception
    {
        public SubsonicApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SubsonicApiException(string message) : base(message)
        {
        }

        protected SubsonicApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class SubsonicInvalidApiException : Exception
    {
        public SubsonicInvalidApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SubsonicInvalidApiException(string message) : base(message)
        {
        }

        protected SubsonicInvalidApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}