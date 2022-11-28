using Audit.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class PersonalFinanceResponse
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [AuditIgnore]
        [Required]
        public Guid EntityFinanceId { get; set; }
        [ForeignKey("EntityFinanceId")]
        public virtual EntityFinance EntityFinance { get; set; }

        [Required]
        public Guid PersonalFinanceAttributeCategoryMappingId { get; set; }
        [ForeignKey("PersonalFinanceAttributeCategoryMappingId")]
        public virtual PersonalFinanceAttributeCategoryMapping PersonalFinanceAttributeCategoryMapping { get; set; }

        [Required]
        public string Answer { get; set; }

        [AuditIgnore]
        [Required]
        public int Order { get; set; }
    }
}
