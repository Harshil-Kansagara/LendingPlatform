using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class ProviderBank : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid EntityId { get; set; }
        [ForeignKey("Entityd")]
        public virtual Entity Entity { get; set; }

        [Required]
        public string BankId { get; set; }

        [Required]
        public string BankName { get; set; }

        [Column(TypeName = "jsonb")]
        public string BankInformationJson { get; set; }

        public Guid IntegratedServiceConfigurationId { get; set; }
        [ForeignKey("IntegratedServiceConfigurationId")]
        public virtual IntegratedServiceConfiguration IntegratedServiceConfiguration { get; set; }

        public virtual List<BankAccountTransaction> BankAccountTransactions { get; set; }
    }
}
