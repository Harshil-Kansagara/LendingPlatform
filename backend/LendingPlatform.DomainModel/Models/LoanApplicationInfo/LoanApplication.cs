using Audit.EntityFramework;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class LoanApplication : BaseModel
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public LoanApplicationStatusType Status { get; set; }

        [Required]
        public decimal LoanAmount { get; set; }

        [Required]
        public Guid LoanPurposeId { get; set; }
        [ForeignKey("LoanPurposeId")]
        public virtual LoanPurpose LoanPurpose { get; set; }

        public Guid? SectionId { get; set; }
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        public Guid? SubLoanPurposeId { get; set; }
        [ForeignKey("SubLoanPurposeId")]
        public virtual SubLoanPurpose SubLoanPurpose { get; set; }

        [Required]
        public decimal LoanPeriod { get; set; }

        [Required]
        [MaxLength(18)]
        public string LoanApplicationNumber { get; set; }

        [AuditIgnore]
        public Guid? LoanAmountDepositeeBankId { get; set; }

        [AuditIgnore]
        public Guid? EMIDeducteeBankId { get; set; }

        [ForeignKey("LoanAmountDepositeeBankId")]
        public virtual EntityBankDetail LoanAmountDepositeeBank { get; set; }

        [ForeignKey("EMIDeducteeBankId")]
        public virtual EntityBankDetail EMIDeducteeBank { get; set; }

        public virtual List<EntityFinance> EntityFinances { get; set; }

        /// <summary>
        /// Fetch the list of EntityLoanApplicationMapping objects.
        /// </summary>
        public virtual List<EntityLoanApplicationMapping> EntityLoanApplicationMappings { get; set; }

        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        [AuditIgnore]
        public string EvaluationComments { get; set; }

        [AuditIgnore]
        public Guid? UpdatedByBankUserId { get; set; }

        [ForeignKey("UpdatedByBankUserId")]
        public BankUser UpdatedByBankUser { get; set; }

        /// <summary>
        /// Interest rate based on self declared credit score of loan initiator.
        /// </summary>
        public decimal? InterestRate { get; set; }

        public virtual List<EntityTaxForm> EntityTaxForms { get; set; }

        public virtual List<CreditReport> CreditReports { get; set; }
        public virtual List<UserLoanSectionMapping> UserLoanSectionMappings { get; set; }

        public virtual List<EntityAdditionalDocument> AdditionalDocuments { get; set; }
    }
}
