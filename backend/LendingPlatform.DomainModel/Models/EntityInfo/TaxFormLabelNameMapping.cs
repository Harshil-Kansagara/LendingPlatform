using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class TaxFormLabelNameMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string LabelFieldName { get; set; }

        public Guid TaxFormId { get; set; }
        [ForeignKey("TaxFormId")]
        public virtual TaxForm TaxForm { get; set; }

        public int Order { get; set; }
    }
}
