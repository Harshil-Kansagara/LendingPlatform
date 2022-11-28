using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityFinanceStandardAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public int Order { get; set; }

        public int? ParentId { get; set; }

        public decimal? ExpectedValue { get; set; }

        [Required]
        public Guid EntityFinancialYearlyMappingId { get; set; }

        [ForeignKey("EntityFinancialYearlyMappingId")]
        public virtual EntityFinanceYearlyMapping EntityFinanceYearlyMapping { get; set; }

        [Column(TypeName = "jsonb")]
        public string SourceJson { get; set; }
    }
}
