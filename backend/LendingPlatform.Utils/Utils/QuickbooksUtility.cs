using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.ReportService;
using Intuit.Ipp.Security;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Quickbooks;
using LendingPlatform.Utils.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class QuickbooksUtility : IQuickbooksUtility
    {
        #region Public methods
        /// <summary>
        /// Method to get authorization url
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="quickbooksConfigurationJson"></param>
        /// <returns></returns>
        public string GetAuthorizationUrl(Guid entityId, string quickbooksConfigurationJson)
        {
            var configuration = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(quickbooksConfigurationJson);
            var _quickbooksAuthClient = new OAuth2Client(configuration.First(x => x.Path == "Quickbooks:ClientId").Value, configuration.First(x => x.Path == "Quickbooks:ClientSecret").Value,
                configuration.First(x => x.Path == "Quickbooks:RedirectUri").Value, configuration.First(x => x.Path == "Quickbooks:AppEnvironment").Value);

            var scopes = new List<OidcScopes>();
            scopes.Add(OidcScopes.Accounting);
            string authorizationUrl = _quickbooksAuthClient.GetAuthorizationURL(scopes, entityId.ToString());
            return authorizationUrl;
        }

        /// <summary>
        /// Fetch Quickbooks reports viz. Income Statement, Balance Sheet
        /// </summary>
        /// <param name="quickbooksConfiguration"></param>
        /// <returns></returns>
        public List<FinancialStatementsAC> FetchQuickbooksReport(ThirdPartyServiceCallbackDataAC quickbooksConfiguration)
        {
            var configuration = quickbooksConfiguration.Configuration;
            var serviceContext = PrepareQuickbooksServiceContext(quickbooksConfiguration.BearerToken, quickbooksConfiguration.RealmId, configuration);
            // Get company info name
            QueryService<CompanyInfo> companyInfoService = new QueryService<CompanyInfo>(serviceContext);
            CompanyInfo companyInfo = companyInfoService.ExecuteIdsQuery("SELECT * FROM CompanyInfo").FirstOrDefault();

            // Prepare report service
            ReportService reportService = new ReportService(serviceContext);



            // Prepare report filter parameters
            reportService.accounting_method = "Accrual";
            reportService.start_date = quickbooksConfiguration.StartDate.ToString("yyyy-MM-dd");
            reportService.end_date = quickbooksConfiguration.EndDate.ToString("yyyy-MM-dd");
            reportService.summarize_column_by = "Year";
            foreach (var reportToFetch in quickbooksConfiguration.ReportListToFetch)
            {
                reportToFetch.ThirdPartyWiseCompanyName = companyInfo.LegalName;
                reportToFetch.ReportJson = JsonConvert.SerializeObject(reportService.ExecuteReport(reportToFetch.ThirdPartyWiseName));
            }
            return quickbooksConfiguration.ReportListToFetch;
        }

        /// <summary>
        /// Fetch Quickbooks chart of account details from accountId list
        /// </summary>
        /// <param name="accountIdList"></param>
        /// <param name="quickbooksTokenAC"></param>
        /// <returns></returns>
        public List<Account> FetchQuickbooksChartOfAccountsById(List<string> accountIdList, ThirdPartyServiceCallbackDataAC quickbooksTokenAC)
        {
            var configuration = quickbooksTokenAC.Configuration;
            QueryService<Account> queryService = new QueryService<Account>(PrepareQuickbooksServiceContext(quickbooksTokenAC.BearerToken, quickbooksTokenAC.RealmId, configuration));
            accountIdList = accountIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();
            // Prepare query and append id parameters
            var accountListQuery = "select * from Account where id in (";
            int listCount = 1;

            foreach (var id in accountIdList)
            {
                if (listCount == accountIdList.Count)
                {
                    accountListQuery = string.Concat(accountListQuery, "'", id, "'", ")");
                }
                else
                {
                    accountListQuery = string.Concat(accountListQuery, "'", id, "'", ",");
                }
                listCount++;
            }
            var accountList = queryService.ExecuteIdsQuery(accountListQuery);
            return accountList.ToList();
        }


        /// <summary>
        /// Method to fetch Quickbooks access token using Quickbooks Authorization Code and Token
        /// </summary>
        /// <param name="quickbooksConfiguration"></param>
        /// <returns></returns>
        public async Task<string> FetchQuickbooksTokensAsync(ThirdPartyServiceCallbackDataAC quickbooksConfiguration)
        {
            var configuration = quickbooksConfiguration.Configuration;
            var _quickbooksAuthClient = new OAuth2Client(configuration.First(x => x.Path == "Quickbooks:ClientId").Value, configuration.First(x => x.Path == "Quickbooks:ClientSecret").Value,
                configuration.First(x => x.Path == "Quickbooks:RedirectUri").Value, configuration.First(x => x.Path == "Quickbooks:AppEnvironment").Value);

            // Check CSRF token and auth code, if proper request bearer token and refresh token from Quickbooks
            if (!String.IsNullOrEmpty(quickbooksConfiguration.CSRFToken) && !string.IsNullOrEmpty(quickbooksConfiguration.AuthorizationCode))
            {
                var accessTokenResponse = await _quickbooksAuthClient.GetBearerTokenAsync(quickbooksConfiguration.AuthorizationCode);
                if (!accessTokenResponse.IsError)
                {
                    quickbooksConfiguration.BearerToken = accessTokenResponse.AccessToken;
                }
                else
                {
                    // Throw exception
                    throw new InvalidDataException(accessTokenResponse.Error);
                }
            }
            else
            {
                // Throw exception
                throw new InvalidDataException(StringConstant.MissingParameters);
            }

            return quickbooksConfiguration.BearerToken;
        }


        /// <summary>
        /// Get accounts with amount
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="quickbooksAccounts"></param>
        /// <returns></returns>
        public List<QuickbooksAccountAC> GetAccountsWithAmountFromReportRows(List<Row> rows, List<QuickbooksAccountAC> quickbooksAccounts)
        {
            foreach (var row in rows)
            {
                if (row.AnyIntuitObjects?.Count() != 3 && row.type == RowTypeEnum.Data)
                {

                    var summaryListForEachSection = (JArray)row.AnyIntuitObjects[0];
                    var accountName = summaryListForEachSection[0]["value"].ToString();

                    List<decimal> amounts = new List<decimal>();
                    decimal amount;

                    foreach (var col in summaryListForEachSection)
                    {
                        if (decimal.TryParse(col["value"]?.ToString(), out amount))
                        {
                            amounts.Add(decimal.TryParse(col["value"]?.ToString(), out amount) ? amount : 0);
                        }
                        else
                        {
                            amounts.Add(0);
                        }
                    }

                    quickbooksAccounts.Add(new QuickbooksAccountAC
                    {
                        Id = summaryListForEachSection[0]["id"]?.ToString(),
                        Group = accountName,
                        Amounts = amounts
                    });
                }
                if (row.AnyIntuitObjects?.Count() == 3)
                {
                    Rows childRows = JsonConvert.DeserializeObject<Rows>(JsonConvert.SerializeObject(row.AnyIntuitObjects[1]));
                    GetAccountsWithAmountFromReportRows(childRows.Row.ToList(), quickbooksAccounts);
                }
            }
            return quickbooksAccounts;
        }
        #endregion


        #region Private methods
        /// <summary>
        /// Method to prepare service context for Quickbooks using bearer token and realmId
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <param name="realmId"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private ServiceContext PrepareQuickbooksServiceContext(string bearerToken, string realmId, List<ThirdPartyConfigurationAC> configuration)
        {
            OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(bearerToken);
            ServiceContext serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, oauthValidator);
            serviceContext.IppConfiguration.BaseUrl.Qbo = configuration.First(x => x.Path == "Quickbooks:BaseUrl").Value;
            serviceContext.IppConfiguration.MinorVersion.Qbo = configuration.First(x => x.Path == "Quickbooks:ConfigurationVersion").Value;
            serviceContext.IppConfiguration.Message.Request.SerializationFormat = SerializationFormat.Json;
            serviceContext.IppConfiguration.Message.Response.SerializationFormat = SerializationFormat.Json;
            return serviceContext;
        }

        #endregion

    }
}
