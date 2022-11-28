using Audit.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class LoanPurpose
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
        public Guid LoanTypeId { get; set; }
        [ForeignKey("LoanTypeId")]
        public virtual LoanType LoanType { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

        public virtual List<LoanPurposeRangeTypeMapping> LoanPurposeRangeTypeMappings { get; set; }
        public virtual List<SubLoanPurpose> SubLoanPurposes { get; set; }
    }
}
