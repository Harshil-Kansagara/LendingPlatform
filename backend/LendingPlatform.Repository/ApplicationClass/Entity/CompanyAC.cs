using LendingPlatform.Repository.ApplicationClass.Others;
using System;
using System.ComponentModel.DataAnnotations;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class CompanyAC
    {
        #region Public Properties
        /// <summary>
        /// Name of the company.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Personal SSN for propietorship or company EIN for other company structure.
        /// </summary>
        [Required]
        public string CIN { get; set; }
        /// <summary>
        /// The company type details
        /// </summary>
        [Required]
        public CompanyStructureAC CompanyStructure { get; set; }
        /// <summary>
        /// Company Age Deteails
        /// </summary>
        [Required]
        public BusinessAgeAC BusinessAge { get; set; }
        /// <summary>
        /// Company size details
        /// </summary>
        [Required]
        public CompanySizeAC CompanySize { get; set; }
        /// <summary>
        /// Industry Type details
        /// </summary>
        [Required]
        public IndustryTypeAC IndustryType { get; set; }
        /// <summary>
        /// Industry Experience details
        /// </summary>
        [Required]
        public IndustryExperienceAC IndustryExperience { get; set; }
        /// <summary>
        /// Company registered state location
        /// </summary>
#nullable enable
        public string? CompanyRegisteredState { get; set; }
#nullable disable
        /// <summary>
        /// Is Company's Fiscal Year Same as Calender
        /// </summary>
        public int? CompanyFiscalYearStartMonth { get; set; }
        /// <summary>
        /// Unique identifier for creator(entity) of the company.
        /// </summary>
        public Guid? CreatedByUserId { get; set; }
        #endregion
    }
}
