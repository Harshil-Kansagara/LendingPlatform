using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityLoanApplicationMapping
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid EntityId { get; set; }
        [ForeignKey("EntityId")]
        public virtual Entity Entity { get; set; }

        [AuditIgnore]
        [Required]
        public Guid LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public virtual LoanApplication LoanApplication { get; set; }
    }
}
