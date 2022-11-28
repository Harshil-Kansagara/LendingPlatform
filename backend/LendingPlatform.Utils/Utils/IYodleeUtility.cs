
using LendingPlatform.Utils.ApplicationClass.Yodlee;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IYodleeUtility
    {
        /// <summary>
        /// Method is to get the yodlee fastlink url
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <returns>YodleeFastLink AC obj</returns>
        Task<YodleeFastLinkAC> GetFastLinkUrlAsync(Guid entityId);

        /// <summary>
        /// Method is to get the provider bank data from yodlee API
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <param name="providerBankId">Pass providerBankId</param>
        /// <returns>Return ProviderBankResponseAC obj</returns>
        Task<ProviderBankResponseAC> GetProviderBankResponseAsync(Guid entityId, string providerBankId);

        /// <summary>
        /// Method is to get the transaction data from yodlee API
        /// </summary>        
        /// /// <param name="entityId">Pass current company Id</param>
        /// <param name="accountIds">Pass accountIds</param>
        /// <returns>Return TransactionDetailResponseAC list</returns>
        Task<List<TransactionDetailResponseAC>> GetTransactionJsonAsync(Guid entityId, string accountIds, string fromDate);

        /// <summary>
        /// Mehtod is to get the account data from yodlee API
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <param name="accountId">Pass AccountId</param>
        /// <returns>Return AccountResponseAC obj</returns>
        Task<AccountResponseAC> GetAccountResponseAsync(Guid entityId, long accountId);

    }
}
