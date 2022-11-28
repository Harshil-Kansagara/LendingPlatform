using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models
{
    public class IntegratedServiceConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public string ConfigurationJson { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsServiceEnabled { get; set; }
    }
}
