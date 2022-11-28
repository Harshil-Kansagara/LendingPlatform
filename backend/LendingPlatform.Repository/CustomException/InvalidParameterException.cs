using System;
using System.Runtime.Serialization;

namespace LendingPlatform.Repository.CustomException
{
    /// <summary>
    /// Custom exception class for throwing exception when supplied data is invalid
    /// </summary>
    [Serializable]
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException()
        {
        }

        public InvalidParameterException(string message) : base(message)
        {
        }

        protected InvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
