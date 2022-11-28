using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;

namespace LendingPlatform.Utils.Utils
{
    public class XeroUtility : IXeroUtility
    {

        private readonly IAccountingApi _accountingApi;


        public XeroUtility(IAccountingApi accountingApi)
        {
            _accountingApi = accountingApi;
        }

        #region  Private methods

        /// <summary>
        /// Bind the xero settings in the XeroConfiguration class.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns>XeroConfiguration class.</returns>
        private XeroConfiguration GetXeroConfiguration(Guid entityId, List<ThirdPartyConfigurationAC> configuration)
        {
            XeroConfiguration xconfig = new XeroConfiguration()
            {
                ClientId = configuration.First(x => x.Path == "Xero:ClientId").Value,
                ClientSecret = configuration.First(x => x.Path == "Xero:ClientSecret").Value,
                CallbackUri = new Uri(configuration.First(x => x.Path == "Xero:CallBackUri").Value),
                Scope = "accounting.reports.read accounting.settings",
                State = entityId.ToString()

            };
            return xconfig;
        }

        /// <summary>
        /// Update the date format for Xero reports header.
        /// </summary>
        /// <param name="reportWithRows">API response report object.</param>
        /// <param name="listOfPeriods">eg{ Jan - Dec 2020 , Jan - Dec 2019 }</param>
        private List<ReportWithRow> UpdateXeroReportsHeaderForDateFormat(List<ReportWithRow> reportWithRows, List<string> listOfPeriods)
        {
            int count = 0;
            foreach (var reportWithRow in reportWithRows)
            {
                var headerRow = reportWithRow.Rows.Find(s => s.RowType == RowType.Header);
                var headerRowYearCells = headerRow.Cells.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();
                if (headerRowYearCells.Count > listOfPeriods.Count)
                    headerRowYearCells.Remove(headerRowYearCells.Last());
                foreach (var cell in headerRowYearCells)
                {
                    if (!string.IsNullOrEmpty(cell.Value))
                    {
                        cell.Value = listOfPeriods[index: count];
                        count++;
                    }
                }
                headerRow.Cells = headerRow.Cells.Where(x => listOfPeriods.Contains(x.Value)).ToList();
            }
            return reportWithRows;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the Xero Login url to redirect on the Xero.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns> Xero Login Url.</returns>
        public string GetLoginUrl(Guid entityId, string xeroConfigurationJson)
        {
            var configuration = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(xeroConfigurationJson);
            // Get Configuratin details.
            XeroConfiguration xconfig = GetXeroConfiguration(entityId, configuration);

            var client = new XeroClient(xconfig);

            // Return Login url.
            return client.BuildLoginUri();
        }

        /// <summary>
        /// Call an XERO api to fetches the balancesheet and profit and loss JSON result.
        /// </summary>
        /// <param name="xeroConfigurationAC">Contains setting of the xero</param>
        /// <returns>Get the balancesheet and profit and loss JSON result.</returns>
        public async Task<List<FinancialStatementsAC>> GetFinancialInfoAsync(ThirdPartyServiceCallbackDataAC xeroConfigurationAC)
        {
            foreach (var reportToFetch in xeroConfigurationAC.ReportListToFetch)
            {
                if (reportToFetch.ReportName.Equals(StringConstant.BalanceSheet))
                {
                    var balanceSheetReport = await _accountingApi.GetReportBalanceSheetAsync(xeroConfigurationAC.BearerToken, xeroConfigurationAC.TenantId.ToString(), date: xeroConfigurationAC.EndDate.ToString("yyyy-MM-dd"), periods: xeroConfigurationAC.LastYears, timeframe: "YEAR", standardLayout: true);
                    reportToFetch.ReportJson = JsonConvert.SerializeObject(UpdateXeroReportsHeaderForDateFormat(balanceSheetReport.Reports, xeroConfigurationAC.PeriodList));
                }
                else if (reportToFetch.ReportName.Equals(StringConstant.IncomeStatement))
                {
                    var endDate = xeroConfigurationAC.EndDate;
                    var profitAndLossReport = await _accountingApi.GetReportProfitAndLossAsync(xeroConfigurationAC.BearerToken, xeroConfigurationAC.TenantId.ToString(), fromDate: endDate.AddDays(-365), toDate: endDate, periods: xeroConfigurationAC.LastYears, timeframe: "YEAR", standardLayout: true);
                    reportToFetch.ReportJson = JsonConvert.SerializeObject(UpdateXeroReportsHeaderForDateFormat(profitAndLossReport.Reports, xeroConfigurationAC.PeriodList));
                }
                reportToFetch.ThirdPartyWiseCompanyName = (await _accountingApi.GetOrganisationsAsync(xeroConfigurationAC.BearerToken, xeroConfigurationAC.TenantId.ToString()))._Organisations.FirstOrDefault(x => x.OrganisationID == Guid.Parse(xeroConfigurationAC.TenantId))?.LegalName;
            }

            return xeroConfigurationAC.ReportListToFetch;
        }

        /// <summary>
        /// Get xero token
        /// </summary>
        /// <param name="xeroConfigurationAC"></param>
        /// <returns></returns>
        public async Task<IXeroToken> GetXeroTokenAsync(ThirdPartyServiceCallbackDataAC xeroConfigurationAC)
        {
            // Get the Xero configuration details as per the appsettings.
            XeroConfiguration xconfig = GetXeroConfiguration(xeroConfigurationAC.EntityId, xeroConfigurationAC.Configuration);
            var client = new XeroClient(xconfig);

            var xeroToken = await client.RequestAccessTokenAsync(xeroConfigurationAC.AuthorizationCode);
            return xeroToken;
        }


        /// <summary>
        /// Get account with their amounts for each year
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public List<XeroAccountAC> GetAccountsWithAmountFromReportRows(List<ReportRows> rows)
        {
            var xeroAccounts = new List<XeroAccountAC>();
            foreach (var row in rows)
            {
                var accountRowList = row.Rows.Where(s => s.RowType == RowType.Row).ToList();
                foreach (var accountRow in accountRowList)
                {

                    var accountId = accountRow.Cells?[0].Attributes?[0].Value;
                    if (!string.IsNullOrEmpty(accountId) && accountRow != null)
                    {

                        List<decimal> amounts = new List<decimal>();
                        decimal amount;

                        foreach (var col in accountRow.Cells)
                        {
                            if (decimal.TryParse(col.Value?.ToString(), out amount))
                            {
                                amounts.Add(decimal.TryParse(col.Value?.ToString(), out amount) ? amount : 0);
                            }
                            else
                            {
                                amounts.Add(0);
                            }
                        }

                        xeroAccounts.Add(new XeroAccountAC
                        {
                            Id = accountId,
                            Name = accountRow.Cells?[0].Value,
                            Amounts = amounts
                        });


                    }

                }

            }
            return xeroAccounts;
        }


        /// <summary>
        /// Fetch chart of accounts based on account id
        /// </summary>
        /// <param name="accountIdList"></param>
        /// <param name="xeroConfigurationAC"></param>
        /// <returns></returns>
        public async Task<List<Account>> FetchXeroChartOfAccountsByIdAsync(List<string> accountIdList, ThirdPartyServiceCallbackDataAC xeroConfigurationAC)
        {
            accountIdList = accountIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();

            var accountListQuery = "AccountID == Guid(";
            var accountFilterList = new List<string>();
            foreach (var id in accountIdList)
            {
                accountFilterList.Add(string.Concat(accountListQuery, "\"", id, "\")"));
            }
            string finalQuery = accountFilterList.First();
            foreach (var id in accountFilterList.Where(x => !x.Equals(finalQuery)))
            {
                finalQuery = string.Concat(finalQuery, "||", id);
            }

            Accounts accounts = await _accountingApi.GetAccountsAsync(xeroConfigurationAC.BearerToken, xeroConfigurationAC.TenantId.ToString(), where: finalQuery);
            return accounts._Accounts;

        }
        #endregion
    }

}
