using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class Section
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Order { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

        public Guid? ParentSectionId { get; set; }

        [ForeignKey("ParentSectionId")]
        public virtual Section ParentSection { get; set; }


        public int SectionId { get; set; }
    }
}
