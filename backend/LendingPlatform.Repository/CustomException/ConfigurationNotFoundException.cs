using System;

namespace LendingPlatform.Repository.CustomException
{
    /// <summary>
    /// Custom exception class for catch exception when configuration from appsettings.json file is not fetched.
    /// </summary>
    [Serializable]
    public class ConfigurationNotFoundException : Exception
    {
        public ConfigurationNotFoundException()
        {

        }

        public ConfigurationNotFoundException(string message) : base(message)
        {

        }
    }
}
