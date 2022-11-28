using System;
using System.ComponentModel.DataAnnotations;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class IndustryTypeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for industry group object
        /// </summary>
        [Required]
        public Guid Id { get; set; }
        /// <summary>
        /// NAICS code for industry type
        /// </summary>
        public string IndustryCode { get; set; }
        /// <summary>
        /// Industry type
        /// </summary>
        public string IndustryType { get; set; }
        /// <summary>
        /// Unique identifier for industry sector object
        /// </summary>
        public Guid? IndustrySectorId { get; set; }
        #endregion
    }
}
