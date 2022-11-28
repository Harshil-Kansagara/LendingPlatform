using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Utils.ApplicationClass;
using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class EntityAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the entity object
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Entity type details
        /// </summary>
        public EntityType? Type { get; set; }
        /// <summary>
        /// Address details of entity
        /// </summary>
        public AddressAC Address { get; set; }
        /// <summary>
        /// User details
        /// </summary>
        public UserAC User { get; set; }
        /// <summary>
        /// Company details
        /// </summary>
        public CompanyAC Company { get; set; }
        /// <summary>
        /// List of linked entities
        /// </summary>
        public List<EntityAC> LinkedEntities { get; set; }
        /// <summary>
        /// List of company finances
        /// </summary>
        public List<CompanyFinanceAC> CompanyFinances { get; set; }
        /// <summary>
        /// Personal finances of entity
        /// </summary>
        public PersonalFinanceAC PersonalFinance { get; set; }
        /// <summary>
        /// Credit report details
        /// </summary>
        public CreditReportAC CreditReport { get; set; }
        /// <summary>
        /// Consent details
        /// </summary>
        public List<ConsentAC> Consents { get; set; }
        /// <summary>
        /// List of taxes related to entity
        /// </summary>
        public List<TaxAC> Taxes { get; set; }

        /// <summary>
        /// List of additional documents of entity
        /// </summary>
        public List<AdditionalDocumentAC> AdditionalDocuments { get; set; }

        /// <summary>
        /// List of all finance and tax years 
        /// </summary>
        public List<string> Periods { get; set; }

        public EntityRelationMappingAC RelationMapping { get; set; }
        /// <summary>
        /// It's loan id. It's used to track the log details of the loan.
        /// </summary>
        public Guid? LoanId { get; set; }
        #endregion
    }
}
