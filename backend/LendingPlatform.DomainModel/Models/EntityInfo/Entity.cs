using Audit.EntityFramework;
using LendingPlatform.DomainModel.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class Entity
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? AddressId { get; set; }
        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }

        [AuditIgnore]
        [Required]
        public EntityType Type { get; set; }

        public virtual User User { get; set; }

        public virtual Company Company { get; set; }

        public virtual List<EntityFinance> EntityFinances { get; set; }
        /// <summary>
        /// CreditReport object.
        /// </summary>
        public virtual List<CreditReport> CreditReports { get; set; }
        /// <summary>
        /// List of bank of the same entity.
        /// </summary>
        public virtual List<ProviderBank> ProviderBanks { get; set; }
        /// <summary>
        /// List of EntityRelationshipMapping objects which are having this object's id as PrimaryEntityId.
        /// </summary>
        [InverseProperty("PrimaryEntity")]
        public virtual List<EntityRelationshipMapping> PrimaryEntityRelationships { get; set; }
        /// <summary>
        /// List of EntityRelationshipMapping objects which are having this object's id as RelativeEntityId.
        /// </summary>
        [InverseProperty("RelativeEntity")]
        public virtual List<EntityRelationshipMapping> RelativeEntityRelationships { get; set; }
        /// <summary>
        /// List of Consent objects which include consents given by this entity.
        /// </summary>
        public virtual List<EntityLoanApplicationConsent> EntityConsents { get; set; }
        /// <summary>
        /// List of tax forms of current entity.
        /// </summary>
        public virtual List<EntityTaxForm> EntityTaxForms { get; set; }

        /// <summary>
        /// List of additional documents of entity.
        /// </summary>
        public virtual List<EntityAdditionalDocument> AdditionalDocuments { get; set; }
    }
}
