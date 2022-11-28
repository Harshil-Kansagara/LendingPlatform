using LendingPlatform.Utils.ApplicationClass.Plaid;
using LendingPlatform.Utils.ApplicationClass.Plaid.Entity;
using LendingPlatform.Utils.ApplicationClass.Plaid.Institution;
using LendingPlatform.Utils.ApplicationClass.Plaid.Management;
using LendingPlatform.Utils.ApplicationClass.Plaid.Transactions;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class PlaidUtility : IPlaidUtility
    {
        private readonly IConfiguration _configuration;
        public PlaidUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region  Private methods
        /// <summary>
        /// Generate access token from plaid.
        /// </summary>
        /// <param name="publicToken">public token</param>
        /// <returns>Access token</returns>
        private async Task<string> CreateAccessTokenAsync(string publicToken)
        {
            // Create access token.
            var result = await ExchangeTokenAsync(new ExchangeTokenRequestAC()
            {
                ClientId = _configuration.GetValue<string>("PlaidService:ClientId"),
                Secret = _configuration.GetValue<string>("PlaidService:ClientSecret"),
                PublicToken = publicToken
            });

            return result.AccessToken;
        }

        /// <summary>
        /// Creates a Link link_token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<CreateLinkTokenResponseAC> CreateLinkTokenAsync(CreateLinkTokenRequestAC request)
        {
            return await PostAsync<CreateLinkTokenResponseAC>("link/token/create", request);
        }
        /// <summary>
        /// Exchanges a Link public_token for an API access_token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;Management.ExchangeTokenResponse&gt;.</returns>
        private async Task<ExchangeTokenResponseAC> ExchangeTokenAsync(ExchangeTokenRequestAC request)
        {
            return await PostAsync<ExchangeTokenResponseAC>("item/public_token/exchange", request);
        }
        /// <summary>
        ///  Retrieves user-authorized transaction data for credit and depository-type <see cref="Entity.Account"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;Transactions.GetTransactionsResponse&gt;.</returns>
        private async Task<GetTransactionsResponseAC> FetchTransactionsAsync(GetTransactionsRequestAC request)
        {
            return await PostAsync<GetTransactionsResponseAC>("transactions/get", request);
        }
        /// <summary>
        /// Retrieves the institutions that match the id.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;Institution.SearchByIdResponse&gt;.</returns>
        private async Task<SearchByIdResponseAC> FetchInstitutionByIdAsync(SearchByIdRequestAC request)
        {
            return await PostAsync<SearchByIdResponseAC>("institutions/get_by_id", request);
        }
        internal string GetEndpoint(string path)
        {
            string subDomain;
            string plaidEnvironment = _configuration.GetValue<string>("PlaidService:Environment");
            if (plaidEnvironment.Equals("Sandbox", StringComparison.InvariantCultureIgnoreCase))
            {
                subDomain = "sandbox.";
            }
            else
            {
                subDomain = plaidEnvironment.Equals("Development", StringComparison.InvariantCultureIgnoreCase)
                    ? "development." : "production.";
            }

            return new UriBuilder()
            {
                Scheme = Uri.UriSchemeHttps,
                Host = $"{subDomain}plaid.com",
                Path = path.Trim(' ', '/', '\\')
            }.Uri.AbsoluteUri;
        }
        private static HttpContent Body(string json)
        {
            return new StringContent(json, Encoding.UTF8, StringConstant.HttpHeaderAcceptJsonType);
        }
        internal async Task<TResponse> PostAsync<TResponse>(string path, SerializableContentAC request) where TResponse : ResponseBaseAC
        {
            using (var http = new HttpClient())
            {
                string url = GetEndpoint(path);
                string json = request.ToJson();

                var body = Body(json);
                body.Headers.Add("Plaid-Version", _configuration.GetValue<string>("PlaidService:Version"));
                using (HttpResponseMessage response = await http.PostAsync(url, body))
                {
                    json = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        TResponse result = JsonConvert.DeserializeObject<TResponse>(json,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                                Formatting = Formatting.Indented
                            });
                        result.StatusCode = response.StatusCode;
                        return result;
                    }
                    else
                    {
                        var error = JObject.Parse(json);
                        throw new HttpRequestException(error["error_message"].Value<string>());
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Generate link token from plaid.
        /// </summary>
        /// <param name="userId">loggedIn userid</param>
        /// <returns>Link token</returns>
        public async Task<string> CreateLinkTokenAsync(Guid userId)
        {
            string[] products = { "auth", "transactions" };
            string[] countryCodes = { "US" };
            // Create link token.
            var result = await CreateLinkTokenAsync(new CreateLinkTokenRequestAC()
            {
                ClientId = _configuration.GetValue<string>("PlaidService:ClientId"),
                Secret = _configuration.GetValue<string>("PlaidService:ClientSecret"),
                ClientName = _configuration.GetValue<string>("PlaidService:ClientName"),
                Products = products,
                CountryCodes = countryCodes,
                Language = "en",
                User = new PlaidUserAC() { ClientUserId = userId.ToString() }
            });

            return result.LinkToken;
        }
        /// <summary>
        /// Fetch the user bank transaction.
        /// </summary>
        /// <param name="publicToken"></param>
        /// <returns>Returns GetTransactionsResponse object contains account and transaction.</returns>
        public async Task<GetTransactionsResponseAC> GetTransactionInfoAsync(string publicToken)
        {
            // Generate access token.
            string accessToken = await CreateAccessTokenAsync(publicToken);

            int lastNYears = _configuration.GetValue<int>("FinancialYear:Years");
            var fromYear = DateTime.UtcNow.Year - lastNYears;
            var fromDate = new DateTime(year: fromYear, month: 1, day: 1);
            var endDate = DateTime.UtcNow;

            uint offset = 0, fetchedRecords = 0, totalFetchRecords = 500;
            GetTransactionsResponseAC getTransactionsResponseAC = new GetTransactionsResponseAC();

            do
            {
                // Retrieving a user's recent transactions.
                var result = await FetchTransactionsAsync(new GetTransactionsRequestAC()
                {
                    ClientId = _configuration.GetValue<string>("PlaidService:ClientId"),
                    Secret = _configuration.GetValue<string>("PlaidService:ClientSecret"),
                    AccessToken = accessToken,
                    StartDate = fromDate,
                    EndDate = endDate,
                    Options = new PaginationOptionsAC()
                    {
                        Offset = offset,
                        Count = totalFetchRecords
                    }
                });
                if (offset == 0)
                {
                    getTransactionsResponseAC = result;
                }
                else
                {
                    getTransactionsResponseAC.Transactions.AddRange(result.Transactions);
                }
                fetchedRecords += totalFetchRecords;
                offset++;
            } while (getTransactionsResponseAC.TotalTransactions > fetchedRecords);

            return getTransactionsResponseAC;
        }
        /// <summary>
        /// Fetch the bank detail.
        /// </summary>
        /// <param name="publicToken">Public token</param>
        /// <param name="institutionId">institutionId like ins0001</param>
        /// <returns>Returns Institution details.</returns>
        public async Task<InstitutionAC> GetBankDetailsAsync(string institutionId)
        {
            // Fetching Institution details.
            var result = await FetchInstitutionByIdAsync(new SearchByIdRequestAC()
            {
                ClientId = _configuration.GetValue<string>("PlaidService:ClientId"),
                Secret = _configuration.GetValue<string>("PlaidService:ClientSecret"),
                InstitutionId = institutionId
            });
            return result.Institution;
        }



    }
}
