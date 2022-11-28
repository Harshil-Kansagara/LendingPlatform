using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class ProductTypeMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public Guid LoanTypeId { get; set; }
        [ForeignKey("LoanTypeId")]
        public virtual LoanType LoanType { get; set; }
    }
}
