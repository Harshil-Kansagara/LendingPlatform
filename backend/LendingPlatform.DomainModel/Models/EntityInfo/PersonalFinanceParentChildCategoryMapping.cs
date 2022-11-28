using Audit.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class PersonalFinanceParentChildCategoryMapping
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ParentCategoryId { get; set; }
        [ForeignKey("ParentCategoryId")]
        public virtual PersonalFinanceCategory ParentCategory { get; set; }

        [Required]
        public Guid ChildCategoryId { get; set; }
        [ForeignKey("ChildCategoryId")]
        public virtual PersonalFinanceCategory ChildCategory { get; set; }

        public Guid? ParentAttributeCategoryMappingId { get; set; }
        [ForeignKey("ParentAttributeCategoryMappingId")]
        public virtual PersonalFinanceAttributeCategoryMapping ParentAttributeCategoryMapping { get; set; }
    }
}
