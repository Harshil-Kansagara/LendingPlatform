using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class CompanyStructure
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Structure { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
