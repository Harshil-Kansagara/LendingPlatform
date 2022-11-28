
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime ProductStartDate { get; set; }

        [Required]
        public DateTime ProductEndDate { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
        public virtual List<DescriptionPoint> DescriptionPoints { get; set; }
        public virtual List<ProductRangeTypeMapping> ProductRangeTypeMappings { get; set; }
        public virtual List<ProductSubPurposeMapping> ProductSubPurposeMappings { get; set; }
    }
}