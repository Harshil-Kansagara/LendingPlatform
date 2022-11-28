using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class ApplicationBasicDetailAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for application
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Application number assigned to particular application.
        /// </summary>
        public string LoanApplicationNumber { get; set; }
        /// <summary>
        /// Unique identifier for the entity object who initiated application.
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }
        /// <summary>
        /// Loan Amount of the application.
        /// </summary>
        [Required]
        public decimal LoanAmount { get; set; }
        /// <summary>
        /// Unique identifier for Loan Purpose object.
        /// </summary>
        [Required]
        public Guid LoanPurposeId { get; set; }
        /// <summary>
        /// Unique identifier for Sub Loan Purpose object.
        /// </summary>
        [Required]
        public Guid SubLoanPurposeId { get; set; }
        /// <summary>
        /// Time period of loan
        /// </summary>
        [Required]
        public decimal LoanPeriod { get; set; }
        /// <summary>
        /// Current status of the application.
        /// </summary>
        public LoanApplicationStatusType Status { get; set; }
        /// <summary>
        /// Current section the application is in.
        /// </summary>
        public string SectionName { get; set; }
        /// <summary>
        /// Application creation date.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Application updation date.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        /// <summary>
        /// Data output from rule engine during rule evaluation
        /// </summary>
        public string EvaluationComments { get; set; }
        /// <summary>
        /// Interest rate based on self declared credit score of loan initiator.
        /// </summary>
        public decimal? InterestRate { get; set; }
        /// <summary>
        /// Details of Bank user who updated the application.
        /// </summary>
        public BankUser UpdatedByBankUser { get; set; }


        /// <summary>
        /// Is application in view only mode or not
        /// </summary>
        public bool IsReadOnlyMode { get; set; }

        /// <summary>
        /// Entity's bank details for the application
        /// </summary>
        public LoanEntityBankDetailsAC EntityBankDetails { get; set; }
        /// <summary>
        /// Date on which the application is last updated
        /// </summary>
        public DateTime? LastUpdatedOn { get; set; }
        /// <summary>
        /// Unique identifier of entity if it is mapped
        /// </summary>
        public Guid MappedEntityId { get; set; }
        /// <summary>
        /// Unique identifier for entity user (Loan initiator)
        /// </summary>
        public Guid CreatedByUserId { get; set; }
        #endregion
    }
}
