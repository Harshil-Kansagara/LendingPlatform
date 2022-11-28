using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class OCRModelMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string ModelId { get; set; }

        public string Year { get; set; }

        public Guid? CompanyStructureId { get; set; }
        [ForeignKey("CompanyStructureId")]
        public virtual CompanyStructure CompanyStructure { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
