namespace LendingPlatform.Utils.ApplicationClass
{
    public class ThirdPartyConfigurationAC
    {
        /// <summary>
        /// Unique Key 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Path format equivalent to appsettings
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Value saved for key
        /// </summary>
        public string Value { get; set; }
    }
}
