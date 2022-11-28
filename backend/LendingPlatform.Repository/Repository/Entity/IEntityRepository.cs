using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public interface IEntityRepository
    {
        /// <summary>
        /// Save the user credit profile information and if it is valid credit profile based on criteria then return true else return false.
        /// </summary>
        /// <param name="entity">EntityAC object</param>
        /// <returns>if it is valid credit profile based on criteria then return true else return false.</returns>
        Task<bool> UpdateUserCreditProfileInformationAsync(EntityAC entity);

        /// <summary>
        /// Method fetches the list of loan applications linked with given entity id.
        /// </summary>
        /// <param name="entityId">Entity id whose applications are to be fetched</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>List of ApplicationBasicDetailAC objects</returns>
        Task<List<ApplicationBasicDetailAC>> GetLoanApplicationsWithBasicDetailsByEntityIdAsync(Guid entityId, CurrentUserAC currentUser);
        /// <summary>
        /// Add or update entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="type"></param>
        /// <returns>object of EntityAC</returns>
        Task<EntityAC> AddOrUpdateEntityAsync(EntityAC entity, CurrentUserAC loggedInUser, string type);

        /// <summary>
        /// Get list of entity
        /// </summary>
        /// <param name="filterModel"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<List<EntityAC>> GetEntityListAsync(FilterModelAC filterModel, CurrentUserAC currentUser);

        /// <summary>
        /// Get an entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<EntityAC> GetEntityAsync(Guid entityId, CurrentUserAC currentUser);
        /// <summary>
        /// Remove linked entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task RemoveLinkEntityAsync(EntityAC entity, CurrentUserAC currentUser);

        /// <summary>
        /// Method to fetch credit report of entity
        /// </summary>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <param name="applicationId">Unique identifier for application</param>
        /// <param name="currentUser">Current User Object</param>
        /// <returns></returns>
        Task FetchCreditReportAsync(Guid entityId, Guid applicationId, CurrentUserAC currentUser);

        /// <summary>
        /// Method to get credit report of an entity
        /// </summary>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <param name="applicationId">Unique identifier for a loan application</param>
        /// <returns></returns>
        Task<EntityAC> GetCreditReportAsync(Guid entityId, Guid? applicationId);

        /// <summary>
        /// Check if entity has any open loan application.
        /// </summary>
        /// <param name="entityId">Unique identifier of entity object</param>
        /// <param name="currentUser">CurrentUserAC object</param>
        /// <returns></returns>
        Task<bool> CheckEntityAllowToStartNewApplicationAsync(Guid entityId, CurrentUserAC currentUser);

        /// <summary>
        /// Method to fetch the additional documents of given entity
        /// </summary>
        /// <param name="id">Entity id</param>
        /// <param name="type">Resource type (company or user)</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>EntityAC object</returns>
        Task<EntityAC> GetAdditionalDocumentsByResourceIdAsync(Guid id, ResourceType type, CurrentUserAC currentUser);

        /// <summary>
        /// Method to save additional documents of an entity
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityAC">EntityAC object</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task SaveAdditionalDocumentOfEntityAsync(Guid entityId, EntityAC entityAC, CurrentUserAC currentUser);
    }
}
