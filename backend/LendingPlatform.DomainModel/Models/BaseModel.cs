using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.EntityInfo;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models
{
    public class BaseModel
    {
        [AuditIgnore]
        [Required]
        public DateTime CreatedOn { get; set; }

        [AuditIgnore]
        public DateTime? UpdatedOn { get; set; }

        [AuditIgnore]
        [Required]
        public Guid CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }

        [AuditIgnore]
        public Guid? UpdatedByUserId { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual User UpdatedByUser { get; set; }
    }
}
