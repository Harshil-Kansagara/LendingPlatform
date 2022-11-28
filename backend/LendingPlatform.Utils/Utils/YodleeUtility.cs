using LendingPlatform.Utils.ApplicationClass.Yodlee;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class YodleeUtility : IYodleeUtility
    {
        #region Private Variables

        private readonly IConfiguration _configuration;
        private readonly string baseUrl, apiVersion;

        #endregion

        #region Constructor

        public YodleeUtility(IConfiguration configuration)
        {
            _configuration = configuration;
            baseUrl = _configuration.GetValue<string>("Yodlee:BaseUrl");
            apiVersion = _configuration.GetValue<string>("Yodlee:ApiVersion");
        }
        #endregion


        #region Public Method

        /// <summary>
        /// Method is to get the yodlee fastlink url
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <returns>YodleeFastLink AC obj</returns>
        public async Task<YodleeFastLinkAC> GetFastLinkUrlAsync(Guid entityId)
        {
            YodleeFastLinkAC yodleeFastLinkAC = new YodleeFastLinkAC()
            {
                AccessToken = await GenerateAccessTokenAsync(entityId),
                FastLinkUrl = _configuration.GetValue<string>("Yodlee:FastLinkUrl")
            };

            return yodleeFastLinkAC;
        }

        /// <summary>
        /// Method is to get the provider bank data from yodlee API
        /// </summary>
        /// <param name="currentUserId">Pass currentUserId</param>
        /// <param name="providerBankId">Pass providerBankId</param>
        /// <returns>Return ProviderBankResponseAC obj</returns>
        public async Task<ProviderBankResponseAC> GetProviderBankResponseAsync(Guid entityId, string providerBankId)
        {
            ProvidersReponseAC providersReponseAC;
            var accessToken = await GenerateAccessTokenAsync(entityId);
            var providersUrl = string.Format("{0}{1}{2}", baseUrl, StringConstant.ProvidersUrl, providerBankId);

            using (var httpClient = new HttpClient())
            {
                using (var providersRequest = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodGet), providersUrl))
                {
                    providersRequest.Headers.TryAddWithoutValidation(StringConstant.ApiVersion, apiVersion);
                    providersRequest.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));

                    var providersResponse = await httpClient.SendAsync(providersRequest);

                    if (providersResponse.IsSuccessStatusCode)
                    {
                        providersReponseAC = await providersResponse.Content.ReadAsAsync<ProvidersReponseAC>();
                        providersReponseAC.Provider[0].BankInformationJson = await providersResponse.Content.ReadAsStringAsync();
                    }
                    else if (providersResponse.StatusCode.Equals(System.Net.HttpStatusCode.InternalServerError))
                    {
                        providersReponseAC = new ProvidersReponseAC();
                    }
                    else
                    {
                        throw new HttpRequestException(StringConstant.UnexpectedStatusCodeProvidersYodlee);
                    }
                }
            }

            return providersReponseAC.Provider[0];
        }

        /// <summary>
        /// Mehtod is to get the account data from yodlee API
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <param name="accountId">Pass AccountId</param>
        /// <returns>Return AccountResponseAC obj</returns>
        public async Task<AccountResponseAC> GetAccountResponseAsync(Guid entityId, long accountId)
        {
            AccountResponseAC accountResponseAC;
            var accessToken = await GenerateAccessTokenAsync(entityId);
            var accountsUrl = string.Format("{0}{1}{2}", baseUrl, StringConstant.AccountsUrl, accountId);

            using (var httpClient = new HttpClient())
            {
                using (var accountsRequest = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodGet), accountsUrl))
                {
                    accountsRequest.Headers.TryAddWithoutValidation(StringConstant.ApiVersion, apiVersion);
                    accountsRequest.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));

                    var accountsResponse = await httpClient.SendAsync(accountsRequest);

                    if (accountsResponse.IsSuccessStatusCode)
                    {
                        accountResponseAC = await accountsResponse.Content.ReadAsAsync<AccountResponseAC>();

                        var accountResponseString = await accountsResponse.Content.ReadAsStringAsync();
                        var accountsJObject = JsonConvert.DeserializeObject<JObject>(accountResponseString);
                        var accountsJArray = JArray.Parse(accountsJObject["account"].ToString());

                        foreach (var account in accountResponseAC.Account)
                        {
                            foreach (var accountArray in accountsJArray)
                            {
                                var accountObject = accountArray.ToObject<JObject>();
                                if (account.Id.ToString() == accountObject["id"].ToString())
                                {
                                    account.AccountInformationJson = JsonConvert.SerializeObject(accountObject);
                                }
                            }
                        }
                    }
                    else if (accountsResponse.StatusCode.Equals(System.Net.HttpStatusCode.InternalServerError))
                    {
                        accountResponseAC = new AccountResponseAC();
                    }
                    else
                    {
                        throw new HttpRequestException(StringConstant.UnexpectedStatusCodeProvidersYodlee);
                    }
                }
            }

            return accountResponseAC;
        }

        /// <summary>
        /// Method is to get the transaction data from yodlee API
        /// </summary>
        /// <param name="curretnUserId">Pass current UserId</param>
        /// <param name="accountIds">Pass accountIds</param>
        /// <returns>Return TransactionDetailResponseAC list</returns>
        public async Task<List<TransactionDetailResponseAC>> GetTransactionJsonAsync(Guid entityId, string accountIds, string fromDate)
        {
            List<TransactionDetailResponseAC> transactionDetailResponseACs = new List<TransactionDetailResponseAC>();
            int skip = 0;
            int count = 500;
            var accessToken = await GenerateAccessTokenAsync(entityId);
            using (var httpClient = new HttpClient())
            {
                while (count == 500)
                {
                    var transactionsUrl = string.Format("{0}{1}?accountId={2}&fromDate={3}&skip={4}", baseUrl, StringConstant.TransactionsUrl, accountIds, fromDate, skip);
                    using (var transactionsRequest = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodGet), transactionsUrl))
                    {
                        transactionsRequest.Headers.TryAddWithoutValidation(StringConstant.ApiVersion, apiVersion);
                        transactionsRequest.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));

                        var transactionsResponse = await httpClient.SendAsync(transactionsRequest);

                        if (transactionsResponse.IsSuccessStatusCode)
                        {
                            var transactionResponseAC = await transactionsResponse.Content.ReadAsAsync<TransactionResponseAC>();
                            transactionDetailResponseACs.AddRange(transactionResponseAC.Transaction);
                            skip = transactionDetailResponseACs.Count;
                            count = transactionResponseAC.Transaction.Count;
                        }
                        else
                        {
                            throw new HttpRequestException(StringConstant.UnexpectedStatusCodeProvidersYodlee);
                        }
                    }
                }
            }

            return transactionDetailResponseACs;
        }

        #endregion


        #region Private Method

        /// <summary>
        /// Method is to generate the Access Token for yodlee API
        /// </summary>
        /// <param name="entityId">Pass current company Id</param>
        /// <returnsToken String</returns>
        /// Here change it to EntityId instead of currentUserId
        private async Task<string> GenerateAccessTokenAsync(Guid entityId)
        {
            string accessToken = null;

            var authUrl = string.Format("{0}{1}", baseUrl, StringConstant.AuthUrl);

            using (var httpClient = new HttpClient())
            {
                var authUrlRequest = GenerateHttpRequestMessage(authUrl, entityId.ToString());

                var currentEntityResponse = await httpClient.SendAsync(authUrlRequest);

                // If response code is be Unauthorized than current user need to register first and afterward we will get the access token for the current user.
                if (currentEntityResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Generate the access token for login Admin
                    var loginAdminName = _configuration.GetValue<string>("Yodlee:LoginAdminName");
                    var adminUrlRequest = GenerateHttpRequestMessage(authUrl, loginAdminName);
                    var adminResponse = await httpClient.SendAsync(adminUrlRequest);

                    if (adminResponse.IsSuccessStatusCode)
                    {
                        var adminResponseString = await adminResponse.Content.ReadAsStringAsync();
                        var adminJObject = JsonConvert.DeserializeObject<JObject>(adminResponseString);
                        var adminAccessToken = adminJObject["token"]["accessToken"].ToString();

                        // Now send the request for registering current user with adding that access token in the header
                        var registerEntityUrl = string.Format("{0}{1}", baseUrl, StringConstant.RegisterUrl);
                        var registerEntityRequest = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), registerEntityUrl);
                        registerEntityRequest.Headers.TryAddWithoutValidation(StringConstant.ApiVersion, apiVersion);
                        registerEntityRequest.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, adminAccessToken));

                        registerEntityRequest.Content = new StringContent("{\n\"user\":{\n\"loginName\":\'" + entityId + "'\"\n }\n}");
                        registerEntityRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);
                        var response = await httpClient.SendAsync(registerEntityRequest);

                        if (response.IsSuccessStatusCode)
                        {
                            var newAuthUrlRequest = GenerateHttpRequestMessage(authUrl, entityId.ToString());
                            var newCurrentEntityResponse = await httpClient.SendAsync(newAuthUrlRequest);
                            if (newCurrentEntityResponse.IsSuccessStatusCode)
                            {
                                var newCurrentEntityResponseString = await newCurrentEntityResponse.Content.ReadAsStringAsync();
                                var jObject = JsonConvert.DeserializeObject<JObject>(newCurrentEntityResponseString);
                                accessToken = jObject["token"]["accessToken"].ToString();
                            }
                        }
                    }
                }
                else
                {
                    var responseString = await currentEntityResponse.Content.ReadAsStringAsync();
                    var jObject = JsonConvert.DeserializeObject<JObject>(responseString);
                    accessToken = jObject["token"]["accessToken"].ToString();
                }

                return accessToken;
            }
        }

        /// <summary>
        /// Method is to generate the httpRequestMessage object
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="entityId">current entityId</param>
        /// <returns>HttpRequestMessage obj</returns>
        private HttpRequestMessage GenerateHttpRequestMessage(string url, string entityId)
        {
            var clientId = _configuration.GetValue<string>("Yodlee:ClientId");
            var secret = _configuration.GetValue<string>("Yodlee:Secret");

            var urlRequest = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), url);
            urlRequest.Headers.TryAddWithoutValidation(StringConstant.ApiVersion, apiVersion);
            var contentList = new List<string>();
            contentList.Add($"clientId={Uri.EscapeDataString(clientId)}");
            contentList.Add($"secret={Uri.EscapeDataString(secret)}");
            urlRequest.Content = new StringContent(string.Join("&", contentList));
            urlRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderFormUrlEncoded);
            urlRequest.Headers.TryAddWithoutValidation(StringConstant.LoginName, entityId);

            return urlRequest;
        }

        #endregion
    }
}
