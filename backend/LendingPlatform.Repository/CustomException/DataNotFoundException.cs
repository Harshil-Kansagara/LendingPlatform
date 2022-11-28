using System;
using System.Runtime.Serialization;

namespace LendingPlatform.Repository.CustomException
{
    /// <summary>
    /// Custom exception class for throwing exception when data is not present in database.
    /// </summary>
    [Serializable]
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
        {
        }

        public DataNotFoundException(string message) : base(message)
        {
        }

        protected DataNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}