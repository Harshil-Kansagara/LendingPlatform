using Audit.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class Address : BaseModel
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string PrimaryNumber { get; set; }

        [Required]
        public string StreetLine { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string StateAbbreviation { get; set; }

        public string StreetSuffix { get; set; }

        public string SecondaryNumber { get; set; }

        public string SecondaryDesignator { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [AuditIgnore]
        [Required]
        [Column(TypeName = "jsonb")]
        public string Response { get; set; }

        [AuditIgnore]
        public Guid IntegratedServiceConfigurationId { get; set; }
        [ForeignKey("IntegratedServiceConfigurationId")]
        public virtual IntegratedServiceConfiguration IntegratedServiceConfiguration { get; set; }
    }
}
