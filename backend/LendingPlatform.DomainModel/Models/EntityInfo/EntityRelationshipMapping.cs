using Audit.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityRelationshipMapping
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // PrimaryEntityId will be the main borrower
        [Required]
        [AuditIgnore]
        public Guid PrimaryEntityId { get; set; }
        [ForeignKey("PrimaryEntityId")]
        public virtual Entity PrimaryEntity { get; set; }

        [Required]
        public Guid RelativeEntityId { get; set; }
        [ForeignKey("RelativeEntityId")]
        public virtual Entity RelativeEntity { get; set; }

        [Required]
        [AuditIgnore]
        public Guid RelationshipId { get; set; }
        [ForeignKey("RelationshipId")]
        public virtual Relationship Relationship { get; set; }

        public decimal? SharePercentage { get; set; }
    }
}
