using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class TaxFormValueLabelMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Value { get; set; }

        public string CorrectedValue { get; set; }

        public float Confidence { get; set; }

        public Guid TaxformLabelNameMappingId { get; set; }
        [ForeignKey("TaxformLabelNameMappingId")]
        public virtual TaxFormLabelNameMapping TaxformLabelNameMapping { get; set; }

        public Guid EntityTaxYearlyMappingId { get; set; }
        [ForeignKey("EntityTaxYearlyMappingId")]
        [JsonIgnore]
        public virtual EntityTaxYearlyMapping EntityTaxYearlyMapping { get; set; }

    }
}
