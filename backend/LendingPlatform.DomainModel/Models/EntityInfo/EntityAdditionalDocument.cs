using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityAdditionalDocument : BaseModel
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
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; }

        [Required]
        public Guid AdditionalDocumentTypeId { get; set; }
        [ForeignKey("AdditionalDocumentTypeId")]
        public virtual AdditionalDocumentType AdditionalDocumentType { get; set; }

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
