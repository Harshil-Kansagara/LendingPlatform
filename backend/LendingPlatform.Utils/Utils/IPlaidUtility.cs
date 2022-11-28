using LendingPlatform.Utils.ApplicationClass.Plaid.Institution;
using LendingPlatform.Utils.ApplicationClass.Plaid.Transactions;
using System;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IPlaidUtility
    {
        /// <summary>
        /// Generate link token from plaid.
        /// </summary>
        /// <param name="userId">loggedIn userid</param>
        /// <returns>Link token</returns>
        Task<string> CreateLinkTokenAsync(Guid userId);
        /// <summary>
        /// Fetch the user bank transaction.
        /// </summary>
        /// <param name="publicToken">Public token</param>
        /// <returns>Returns GetTransactionsResponse object contains account and transaction.</returns>
        Task<GetTransactionsResponseAC> GetTransactionInfoAsync(string publicToken);
        /// <summary>
        /// Fetch the bank detail.
        /// </summary>
        /// <param name="institutionId">institutionId like ins0001</param>
        /// <returns>Returns Institution details.</returns>
        Task<InstitutionAC> GetBankDetailsAsync(string institutionId);
    }
}
