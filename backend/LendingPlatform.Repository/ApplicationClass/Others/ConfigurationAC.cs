using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class ConfigurationAC
    {
        /// <summary>
        /// List of enabled sections
        /// </summary>
        public List<SectionAC> Sections { get; set; }

        /// <summary>
        /// List of enabled third party services
        /// </summary>
        public List<string> ThirdPartyServices { get; set; }

        /// <summary>
        /// List of the fields selected from appSettings.json file
        /// </summary>
        public List<AppSettingAC> AppSettings { get; set; }
    }
}
