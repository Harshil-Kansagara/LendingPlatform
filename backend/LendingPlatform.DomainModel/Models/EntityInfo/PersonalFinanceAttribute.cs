using Audit.EntityFramework;
using LendingPlatform.DomainModel.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class PersonalFinanceAttribute
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Text { get; set; }

        [AuditIgnore]
        [Required]
        public PersonalFinanceAttributeFieldType FieldType { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

    }
}
