using LendingPlatform.Utils.ApplicationClass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;

namespace LendingPlatform.Utils.Utils
{
    public interface IXeroUtility
    {

        /// <summary>
        /// Get the Xero Login url to redirect on the Xero.
        /// </summary>
        /// <returns> Xero Login Url.</returns>
        string GetLoginUrl(Guid entityId, string xeroConfigurationJson);

        /// <summary>
        /// Call an XERO api to fetches the balancesheet and profit and loss report.
        /// </summary>
        /// <param name="xeroConfigurationAC">Contains settings of the xero</param>
        /// <returns>Get the balancesheet and profit and loss Report.</returns>
        Task<List<FinancialStatementsAC>> GetFinancialInfoAsync(ThirdPartyServiceCallbackDataAC xeroConfigurationAC);

        /// <summary>
        /// Get xero token
        /// </summary>
        /// <param name="xeroConfigurationAC"></param>
        /// <returns></returns>
        Task<IXeroToken> GetXeroTokenAsync(ThirdPartyServiceCallbackDataAC xeroConfigurationAC);

        /// <summary>
        /// Get account with their amounts for each year
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        List<XeroAccountAC> GetAccountsWithAmountFromReportRows(List<ReportRows> rows);


        /// <summary>
        /// Fetch chart of accounts based on account id
        /// </summary>
        /// <param name="accountIdList"></param>
        /// <param name="xeroConfigurationAC"></param>
        /// <returns></returns>
        Task<List<Account>> FetchXeroChartOfAccountsByIdAsync(List<string> accountIdList, ThirdPartyServiceCallbackDataAC xeroConfigurationAC);
    }
}
