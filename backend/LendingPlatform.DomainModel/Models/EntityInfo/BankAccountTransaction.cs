using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class BankAccountTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ProviderBankId { get; set; }
        [ForeignKey("ProviderBankId")]
        public virtual ProviderBank ProviderBank { get; set; }

        [Required]
        public string AccountId { get; set; }

        [Required]
        public string AccountName { get; set; }

        [Required]
        public decimal CurrentBalance { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Column(TypeName = "jsonb")]
        public string AccountInformationJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string TransactionInformationJson { get; set; }
    }
}
