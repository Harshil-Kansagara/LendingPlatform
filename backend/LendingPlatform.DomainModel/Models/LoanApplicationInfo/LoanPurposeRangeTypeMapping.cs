using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class LoanPurposeRangeTypeMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid LoanPurposeId { get; set; }
        [ForeignKey("LoanPurposeId")]
        public virtual LoanPurpose LoanPurpose { get; set; }

        [Required]
        public Guid LoanRangeTypeId { get; set; }
        [ForeignKey("LoanRangeTypeId")]
        public virtual LoanRangeType LoanRangeType { get; set; }

        [Required]
        public decimal Minimum { get; set; }

        [Required]
        public decimal Maximum { get; set; }

        [Required]
        public decimal StepperAmount { get; set; }
    }
}
