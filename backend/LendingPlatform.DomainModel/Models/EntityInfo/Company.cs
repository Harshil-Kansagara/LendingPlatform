using Audit.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class Company : BaseModel
    {
        [AuditIgnore]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public virtual Entity Entity { get; set; }

        [Required]
        public string CIN { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid CompanyStructureId { get; set; }
        [ForeignKey("CompanyStructureId")]
        public virtual CompanyStructure CompanyStructure { get; set; }

        [Required]
        public Guid NAICSIndustryTypeId { get; set; }
        [ForeignKey("NAICSIndustryTypeId")]
        public virtual NAICSIndustryType NAICSIndustryType { get; set; }

        [Required]
        public Guid BusinessAgeId { get; set; }
        [ForeignKey("BusinessAgeId")]
        public virtual BusinessAge BusinessAge { get; set; }

        [Required]
        public Guid CompanySizeId { get; set; }
        [ForeignKey("CompanySizeId")]
        public virtual CompanySize CompanySize { get; set; }
        [Required]
        public Guid IndustryExperienceId { get; set; }
        [ForeignKey("IndustryExperienceId")]
        public virtual IndustryExperience IndustryExperience { get; set; }
        public string CompanyRegisteredState { get; set; }

        // Add regex here for Company Fiscal Year Start Month to be 1-12 or else make a enum from Jan-Dec
        public int? CompanyFiscalYearStartMonth { get; set; }
    }
}
