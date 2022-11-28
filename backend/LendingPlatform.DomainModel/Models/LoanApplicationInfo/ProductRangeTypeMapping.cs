using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class ProductRangeTypeMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public Guid RangeTypeId { get; set; }
        [ForeignKey("RangeTypeId")]
        public virtual LoanRangeType LoanRangeType { get; set; }

        [Required]
        public decimal Minimum { get; set; }

        [Required]
        public decimal Maximum { get; set; }
    }
}
