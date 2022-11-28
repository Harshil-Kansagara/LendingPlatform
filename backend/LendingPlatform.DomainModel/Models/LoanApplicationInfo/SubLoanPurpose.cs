using Audit.EntityFramework;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class SubLoanPurpose
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [AuditIgnore]
        [Required]
        public int Order { get; set; }

        [AuditIgnore]
        [Required]
        [ForeignKey("LoanPurposeId")]
        public Guid LoanPurposeId { get; set; }
        public virtual LoanPurpose LoanPurpose { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
