using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class CreditReport
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [AuditIgnore]
        [Required]
        public Guid EntityId { get; set; }
        [ForeignKey("EntityId")]
        [AuditIgnore]
        public virtual Entity Entity { get; set; }

        public bool IsBankrupted { get; set; }

        public bool HasPendingLien { get; set; }

        public bool HasPendingJudgment { get; set; }

        public decimal? FsrScore { get; set; }

        public decimal? CommercialScore { get; set; }

        [AuditIgnore]
        [Required]
        public string Response { get; set; }
        [Required]
        public Guid IntegratedServiceConfigurationId { get; set; }
        [AuditIgnore]
        [ForeignKey("IntegratedServiceConfigurationId")]
        public virtual IntegratedServiceConfiguration IntegratedServiceConfiguration { get; set; }

        [AuditIgnore]
        [Required]
        public DateTime CreatedOn { get; set; }


        [AuditIgnore]
        public Guid? LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public virtual LoanApplication LoanApplication { get; set; }
        [AuditIgnore]
        public Guid? Version { get; set; }
        [AuditIgnore]
        public int SurrogateId { get; set; }
    }
}
