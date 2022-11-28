using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityFinanceYearlyMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Period { get; set; }

        [Required]
        public Guid EntityFinanceId { get; set; }
        [ForeignKey("EntityFinanceId")]
        public virtual EntityFinance EntityFinance { get; set; }

        public Guid? UploadedDocumentId { get; set; }

        [ForeignKey("UploadedDocumentId")]
        public virtual Document UploadedFinancialDocument { get; set; }

        public virtual List<EntityFinanceStandardAccount> EntityFinanceStandardAccounts { get; set; }

        public DateTime LastAddedDateTime { get; set; }
    }
}
