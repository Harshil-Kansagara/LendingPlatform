using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class EntityFinance : BaseModel
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [AuditIgnore]
        [Required]
        public Guid EntityId { get; set; }
        [ForeignKey("EntityId")]
        public virtual Entity Entity { get; set; }

        [AuditIgnore]
        [Column(TypeName = "jsonb")]
        public string FinancialInformationJson { get; set; }

        [Required]
        public Guid FinancialStatementId { get; set; }
        [ForeignKey("FinancialStatementId")]
        public virtual FinancialStatement FinancialStatement { get; set; }

        public virtual List<EntityFinanceYearlyMapping> EntityFinanceYearlyMappings { get; set; }

        public Guid? IntegratedServiceConfigurationId { get; set; }
        [ForeignKey("IntegratedServiceConfigurationId")]
        public virtual IntegratedServiceConfiguration IntegratedServiceConfiguration { get; set; }

        [AuditIgnore]
        public string ThirdPartyWiseCompanyName { get; set; }
        [AuditIgnore]
        public bool IsDataEmpty { get; set; }

        [AuditIgnore]
        public Guid? LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public virtual LoanApplication LoanApplication { get; set; }
        [AuditIgnore]
        public Guid? Version { get; set; }
        [AuditIgnore]
        public int SurrogateId { get; set; }

        public virtual List<PersonalFinanceResponse> PersonalFinanceResponses { get; set; }
    }
}
