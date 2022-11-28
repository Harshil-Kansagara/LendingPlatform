using Audit.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityTaxYearlyMapping
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Period { get; set; }

        [AuditIgnore]
        [Required]
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public virtual Document UploadedDocument { get; set; }

        [AuditIgnore]
        [Required]
        public Guid EntityTaxFormId { get; set; }
        [ForeignKey("EntityTaxFormId")]
        public virtual EntityTaxForm EntityTaxForm { get; set; }

        public virtual List<TaxFormValueLabelMapping> TaxFormValueLabelMappings { get; set; }
    }
}