using Audit.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class PersonalFinanceCategory
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [AuditIgnore]
        [Required]
        public Guid PersonalFinanceAccountId { get; set; }
        [ForeignKey("PersonalFinanceAccountId")]
        public virtual PersonalFinanceAccount PersonalFinanceAccount { get; set; }

        [Required]
        public string Name { get; set; }

        [AuditIgnore]
        [Required]
        public int Order { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

        [InverseProperty("ParentCategory")]
        public virtual List<PersonalFinanceParentChildCategoryMapping> MappedAsParentCategoryMappings { get; set; }

        [InverseProperty("ChildCategory")]
        public virtual List<PersonalFinanceParentChildCategoryMapping> MappedAsChildCategoryMappings { get; set; }

        public virtual List<PersonalFinanceAttributeCategoryMapping> PersonalFinanceAttributeCategoryMappings { get; set; }
    }
}
