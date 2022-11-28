using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityTaxForm : BaseModel
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [AuditIgnore]
        [Required]
        public Guid EntityId { get; set; }
        [ForeignKey("EntityId")]
        public virtual Entity Entity { get; set; }

        [AuditIgnore]
        [Required]
        public Guid TaxFormId { get; set; }
        [ForeignKey("TaxFormId")]
        public virtual TaxForm TaxForm { get; set; }

        public virtual List<EntityTaxYearlyMapping> EntityTaxYearlyMappings { get; set; }


        [AuditIgnore]
        public Guid? LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public virtual LoanApplication LoanApplication { get; set; }
        [AuditIgnore]
        public Guid? Version { get; set; }
        [AuditIgnore]
        public int SurrogateId { get; set; }
    }
}
