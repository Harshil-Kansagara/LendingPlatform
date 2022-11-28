using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public interface IEntityTaxReturnRepository
    {
        /// <summary>
        /// Method to get the tax return documents by entity id
        /// </summary>
        /// <param name="id">Entity id or application Id</param>
        /// <param name="type">Enum of ResourceType can be company or loan type</param>
        /// <param name="currentUser">Current user</param>
        /// <returns>EntityAC object</returns>
        Task<EntityAC> GetTaxReturnInfoAsync(Guid id, ResourceType type, CurrentUserAC currentUser);

        /// <summary>
        /// Method to add or update the tax return documents
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityAC">EntityAC object</param>
        /// <param name="currentUser">Current User</param>
        /// <returns></returns>
        Task SaveTaxReturnInfoAsync(Guid entityId, EntityAC entityAC, CurrentUserAC currentUser);

        /// <summary>
        /// Method is to update the tax return value in the DB
        /// </summary>
        /// <param name="applicationId">application unique identifier</param>
        /// <param name="documentId">Document unique identifier</param>
        /// <param name="taxFormValueLabelMappings">List of TaxFormValueLabelMapping obj</param>
        /// <param name="currentUser">Current User</param>
        /// <returns>Updated value of TaxFormValueLabelMappingAC list object</returns>
        Task<List<TaxFormValueLabelMappingAC>> UpdateTaxFormValueAsync(Guid applicationId, Guid documentId, List<TaxFormValueLabelMappingAC> taxFormValueLabelMappings, CurrentUserAC currentUser);

        /// <summary>
        /// Method is to get the list of extracted values from the document
        /// </summary>
        /// <param name="loanApplicationId">LoanApplication Id</param>
        /// <param name="documentId">Document Id</param>
        /// <param name="currentUser">Current User</param>
        /// <returns>List of TaxFormValueLabelMappingAC object</returns>
        Task<List<TaxFormValueLabelMappingAC>> GetExtractedValuesOfDocumentAsync(Guid loanApplicationId, Guid documentId, CurrentUserAC currentUser);

        /// <summary>
        /// Method is to get the taxes data from the DB
        /// </summary>
        /// <param name="taxes">List of TaxAC object</param>
        /// <param name="entityTaxForms">List of EntityTaxForms object</param>
        /// <returns>List of TaxAC project</returns>
        List<TaxAC> GetTaxes(List<TaxAC> taxes, List<EntityTaxForm> entityTaxForms);
    }
}
