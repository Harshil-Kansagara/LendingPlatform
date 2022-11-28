using System;
using System.Runtime.Serialization;

namespace LendingPlatform.Repository.CustomException
{
    [Serializable]
    public class InvalidResourceAccessException : Exception
    {
        public InvalidResourceAccessException()
        {
        }

        public InvalidResourceAccessException(string message) : base(message)
        {
        }

        public InvalidResourceAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidResourceAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
