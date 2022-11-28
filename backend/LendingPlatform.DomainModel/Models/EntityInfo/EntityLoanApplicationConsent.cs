using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityLoanApplicationConsent : BaseModel
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [AuditIgnore]
        [Required]
        public Guid ConsenteeId { get; set; }
        [ForeignKey("ConsenteeId")]
        public virtual Entity Entity { get; set; }

        [AuditIgnore]
        [Required]
        public Guid LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public virtual LoanApplication LoanApplication { get; set; }

        [Required]
        public Guid ConsentId { get; set; }
        [ForeignKey("ConsentId")]
        public virtual Consent Consent { get; set; }

        [AuditIgnore]
        [Required]
        public bool IsConsentGiven { get; set; }
    }
}
