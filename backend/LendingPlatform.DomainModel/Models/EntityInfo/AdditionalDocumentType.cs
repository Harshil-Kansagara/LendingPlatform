using Audit.EntityFramework;
using LendingPlatform.DomainModel.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class AdditionalDocumentType
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Type { get; set; }

        [AuditIgnore]
        [Required]
        public ResourceType DocumentTypeFor { get; set; }

        [AuditIgnore]
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
