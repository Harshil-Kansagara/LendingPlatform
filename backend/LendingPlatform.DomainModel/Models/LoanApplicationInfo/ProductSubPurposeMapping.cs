using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class ProductSubPurposeMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public Guid SubLoanPurposeId { get; set; }
        [ForeignKey("SubLoanPurposeId")]
        public virtual SubLoanPurpose SubLoanPurpose { get; set; }
    }
}
