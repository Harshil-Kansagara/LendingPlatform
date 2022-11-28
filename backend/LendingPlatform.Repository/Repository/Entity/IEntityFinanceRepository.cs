using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Utils.ApplicationClass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public interface IEntityFinanceRepository
    {
        /// <summary>
        /// Get third party finances
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="statementNames"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<List<CompanyFinanceAC>> GetFinancesAsync(Guid id, ResourceType type, string statementNames, CurrentUserAC currentUser);

        /// <summary>
        /// Add or update entity's finances from third party services
        /// </summary>
        /// <param name="redirectUrlCallbackData"></param>
        /// <param name="statementsCsv"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task AddOrUpdateFinancesAsync(ThirdPartyServiceCallbackDataAC redirectUrlCallbackData, string statementsCsv, CurrentUserAC currentUser);

        /// <summary>
        /// Method to fetch authorization url for third party source
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="source"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<string> GetAuthorizationUrlAsync(Guid entityId, string source, CurrentUserAC currentUser);

        /// <summary>
        /// Map standard chart of accounts
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task MapToStandardChartOfAccountsAsync(Guid entityId);

        /// <summary>
        /// Method to fetch mapped standard list for display in frontend
        /// </summary>
        /// <param name="financeAcList"></param>
        /// <param name="entityFinanceList"></param>
        /// <returns></returns>
        List<CompanyFinanceAC> GetStandardAccountsList(List<CompanyFinanceAC> financeAcList, List<EntityFinance> entityFinanceList);

        /// <summary>
        /// Method to get the personal finaces
        /// </summary>
        /// <param name="entityId">Entity id</param>
        /// <param name="scopeCsv">Comma separated list of required data</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>PersonalFinanceAC object</returns>
        Task<PersonalFinanceAC> GetPersonalFinancesAsync(Guid entityId, string scopeCsv, CurrentUserAC currentUser);

        /// <summary>
        /// Method to add or update personal finances
        /// </summary>
        /// <param name="entityId">Id of user</param>
        /// <param name="categoryAC">Category object containing attributes and answers</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task SavePersonalFinancesAsync(Guid entityId, PersonalFinanceCategoryAC categoryAC, CurrentUserAC currentUser);

        /// <summary>
        /// Method to fetch personal finances of all shareholders' linked with given loan application.
        /// </summary>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="scopeCsv">List of scopes of required data</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task<List<EntityAC>> FetchPersonalFinancesForApplicationAsync(Guid applicationId, string scopeCsv, CurrentUserAC currentUser);
    }
}
