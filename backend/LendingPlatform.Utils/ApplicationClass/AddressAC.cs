using System;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class AddressAC
    {
        #region Public properties
        /// <summary>
        /// Unique identifier for address object.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// House number
        /// </summary>
        public string PrimaryNumber { get; set; }
        /// <summary>
        /// Street line
        /// </summary>

        public string StreetLine { get; set; }
        /// <summary>
        /// City
        /// </summary>

        public string City { get; set; }
        /// <summary>
        /// State Abbreviation
        /// </summary>

        public string StateAbbreviation { get; set; }
        /// <summary>
        /// Street suffix
        /// </summary>
        public string StreetSuffix { get; set; }
        /// <summary>
        /// Additional house number
        /// </summary>
        public string SecondaryNumber { get; set; }
        /// <summary>
        /// secondary designator information.
        /// </summary>
        public string SecondaryDesignator { get; set; }
        /// <summary>
        /// Zip code.
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// JSON string of address.
        /// </summary>
        public string AddressJson { get; set; }
        /// <summary>
        /// Unique identifier for integrated service configuration
        /// </summary>
        public Guid? IntegratedServiceConfigurationId { get; set; }

        public List<object> AddressSuggestion { get; set; }
        #endregion
    }
}
