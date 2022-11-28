using System;
using System.Runtime.Serialization;

namespace LendingPlatform.Repository.CustomException
{
    [Serializable]
    public class CliamNotFoundException : Exception
    {
        public CliamNotFoundException()
        {
        }

        public CliamNotFoundException(string message) : base(message)
        {
        }

        public CliamNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CliamNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}

