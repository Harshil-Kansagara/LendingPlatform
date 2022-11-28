using Intuit.Ipp.Data;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Quickbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IQuickbooksUtility
    {
        /// <summary>
        /// Get auth url
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="quickbooksConfigurationJson"></param>
        /// <returns></returns>
        string GetAuthorizationUrl(Guid entityId, string quickbooksConfigurationJson);

        /// <summary>
        /// Get accounts with amount
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="quickbooksAccounts"></param>
        /// <returns></returns>
        List<QuickbooksAccountAC> GetAccountsWithAmountFromReportRows(List<Row> rows, List<QuickbooksAccountAC> quickbooksAccounts);

        /// <summary>
        /// Fetch Quickbooks reports viz. Income Statement, Balance Sheet
        /// </summary>
        /// <param name="quickbooksConfiguration"></param>
        /// <returns></returns>
        List<FinancialStatementsAC> FetchQuickbooksReport(ThirdPartyServiceCallbackDataAC quickbooksConfiguration);

        /// <summary>
        /// Fetch Quickbooks chart of account details from accountId list
        /// </summary>
        /// <param name="accountIdList"></param>
        /// <param name="quickbooksTokenAC"></param>
        /// <returns></returns>
        List<Account> FetchQuickbooksChartOfAccountsById(List<string> accountIdList, ThirdPartyServiceCallbackDataAC quickbooksTokenAC);

        /// <summary>
        /// Method to fetch Quickbooks access token using Quickbooks Authorization Code and Token
        /// </summary>
        /// <param name="quickbooksConfiguration"></param>
        /// <returns></returns>
        Task<string> FetchQuickbooksTokensAsync(ThirdPartyServiceCallbackDataAC quickbooksConfiguration);
    }
}
