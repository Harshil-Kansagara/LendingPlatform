using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class NAICSIndustryType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string IndustryType { get; set; }
        [Required]
        public string IndustryCode { get; set; }
        public Guid? NAICSParentSectorId { get; set; }
        [ForeignKey("NAICSParentSectorId")]
        public virtual NAICSIndustryType NAICSParentSector { get; set; }
    }
}
