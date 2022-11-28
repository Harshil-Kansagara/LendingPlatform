using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class TaxFormCompanyStructureMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid CompanyStructureId { get; set; }
        [ForeignKey("CompanyStructureId")]
        public virtual CompanyStructure CompanyStructure { get; set; }

        [Required]
        public Guid TaxFormId { get; set; }
        [ForeignKey("TaxFormId")]
        public virtual TaxForm TaxForm { get; set; }

        [Required]
        public bool IsSoleProprietors { get; set; }
    }
}