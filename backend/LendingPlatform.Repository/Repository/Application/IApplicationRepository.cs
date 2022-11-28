using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.Application
{
    public interface IApplicationRepository
    {
        /// <summary>
        /// Method fetches all loan applications.
        /// </summary>
        /// <param name="filterModel"></param>
        /// <param name="currentUser">Logged in user</param>
        /// <returns>PagedLoanApplicationsAC objects</returns>
        Task<PagedLoanApplicationsAC> GetAllLoanApplicationsAsync(FilterModelAC filterModel, CurrentUserAC currentUser);

        /// <summary>
        /// Method fetches all details of a particular loan application.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="currentUser">Logged in user</param>
        /// <returns>ApplicationDetailAC object</returns>
        Task<ApplicationAC> GetLoanApplicationDetailsByIdAsync(Guid loanApplicationId, CurrentUserAC currentUser);

        /// <summary>
        /// Method adds or updates loan application in database.
        /// </summary>
        /// <param name="loanApplicationBasicDetailAC">LoanApplicationBasicDetailAC object</param>
        /// <param name="currentUser">Logged in user</param>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <returns>Returns updated object of LoanApplicationBasicDetailAC</returns>
        Task<ApplicationBasicDetailAC> SaveLoanApplicationAsync(ApplicationBasicDetailAC loanApplicationBasicDetailAC, CurrentUserAC currentUser, Guid? loanApplicationId);

        /// <summary>
        /// Fetch the bank details by loan id.
        /// </summary>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>Bank details in LoanEntityBankDetailsAC object</returns>
        Task<LoanEntityBankDetailsAC> GetBankDetailsByLoanIdAsync(Guid applicationId, CurrentUserAC currentUser);

        /// <summary>
        /// Add or update bank details for approved loan.
        /// </summary>
        /// <param name="entityBankDetailsAC">EntityBankDetailsAC object</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task AddOrUpdateBankDetailsAsync(LoanEntityBankDetailsAC entityBankDetailsAC, CurrentUserAC currentUser);

        /// <summary>
        /// Link Loan application with borrowing entity
        /// </summary>
        /// <param name="applicationId">Unique identifier of application object</param>
        /// <param name="borrowingEntityId">Unique identifier of entity object</param>
        /// <param name="currentUser">CurrentUserAC object</param>
        /// <returns></returns>
        Task LinkApplicationWithEntityAsync(Guid applicationId, Guid borrowingEntityId, CurrentUserAC currentUser);

        /// <summary>
        /// Method saves the consent of a user for a given loan application.
        /// </summary>
        /// <param name="applicationId">Loan application id for which the user's consent is to be saved</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task SaveLoanConsentOfUserAsync(Guid applicationId, CurrentUserAC currentUser);

        /// <summary>
        /// Method sends a reminder email to shareholders who have not given consent for loan application yet.
        /// </summary>
        /// <param name="loanApplicationId">Nullable loan application id</param>
        /// <returns></returns>
        Task SendReminderEmailToShareHoldersAsync(Guid? loanApplicationId);

        /// <summary>
        /// Method locks the loan application with its current details.
        /// </summary>
        /// <param name="applicationId">Id of the loan application which needs to locked</param>
        /// <param name="entities">List of entities(shareholders) with their finance summary JSON</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task LockLoanApplicationByIdAsync(Guid applicationId, List<EntityAC> entities, CurrentUserAC currentUser);


        /// <summary>
        /// Evaluate Loan and get loan status
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<ApplicationBasicDetailAC> EvaluateLoanAsync(Guid loanId, CurrentUserAC currentUser);

        /// <summary>
        /// Update loan application status
        /// </summary>
        /// <param name="loanApplication"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<ApplicationBasicDetailAC> UpdateLoanApplicationStatusAsync(ApplicationBasicDetailAC loanApplication, CurrentUserAC currentUser);

        /// <summary>
        /// Delete loan applications by authorized bank users
        /// </summary>
        /// <param name="currentUser"></param>
        Task DeleteLoanApplicationsAsync(CurrentUserAC currentUser);
    }
}
