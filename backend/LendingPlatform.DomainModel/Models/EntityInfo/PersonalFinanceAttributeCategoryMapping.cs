using Audit.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class PersonalFinanceAttributeCategoryMapping
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [AuditIgnore]
        [Required]
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual PersonalFinanceCategory Category { get; set; }

        [Required]
        public Guid AttributeId { get; set; }
        [ForeignKey("AttributeId")]
        public virtual PersonalFinanceAttribute Attribute { get; set; }

        [AuditIgnore]
        public Guid? ParentAttributeCategoryMappingId { get; set; }
        [ForeignKey("ParentAttributeCategoryMappingId")]
        public virtual PersonalFinanceAttributeCategoryMapping ParentAttributeCategoryMapping { get; set; }

        [AuditIgnore]
        [Required]
        public int Order { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(false)]
        public bool IsOriginal { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(false)]
        public bool IsCurrent { get; set; }
        [AuditIgnore]
        public Guid? PersonalFinanceConstantId { get; set; }
        [ForeignKey("PersonalFinanceConstantId")]
        public virtual PersonalFinanceConstant PersonalFinanceConstant { get; set; }

        public virtual List<PersonalFinanceAttributeCategoryMapping> ChildAttributeCategoryMappings { get; set; }

        public virtual List<PersonalFinanceParentChildCategoryMapping> ParentChildCategoryMappings { get; set; }

        public virtual List<PersonalFinanceResponse> PersonalFinanceResponses { get; set; }
    }
}
