using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.PayPal;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class PayPalUtility : IPayPalUtility
    {
        #region Private variables
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public PayPalUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Private method(s)
        /// <summary>
        /// Method to get access token from authorization code.
        /// </summary>
        /// <param name="authorizationCode">Authorization code</param>
        /// <returns>Access token string</returns>
        private async Task<string> GetPayPalAccessTokenAsync(string authorizationCode)
        {
            //Create HttpClient.
            var handler = new HttpClientHandler
            {
                UseCookies = false
            };

            TokenResponseAC tokenResponse;
            using var httpClient = new HttpClient(handler);
            using var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), _configuration.GetValue<string>("PayPal:OauthTokenApi"));

            //Convert credentials into Base64 string for authorization of type BasicAuth.
            var base64Authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(StringConstant.Base64Authorization, _configuration.GetValue<string>("PayPal:ClientId"), _configuration.GetValue<string>("PayPal:Secret"))));
            request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationBasicAuth, base64Authorization));

            var contentList = new List<string>
            {
                $"grant_type={Uri.EscapeDataString(StringConstant.HttpGrantTypeAuthorizationCode)}",
                $"code={Uri.EscapeDataString(authorizationCode)}"
            };
            request.Content = new StringContent(string.Join("&", contentList));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderFormUrlEncoded);

            //Send request and check the status of it.
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Deserialize the response and return access token from it.
            tokenResponse = JsonConvert.DeserializeObject<TokenResponseAC>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                Formatting = Formatting.Indented
            });
            return tokenResponse.AccessToken;
        }

        /// <summary>
        /// Method to fetch the invoices with required parameters (with changing values of params).
        /// </summary>
        /// <param name="httpClient">HttpClient object</param>
        /// <param name="searchInvoicesRequest">SearchInvoicesRequestAC object</param>
        /// <param name="accessToken">Access token string</param>
        /// <param name="pageNumber">Page number</param>
        /// <returns>InoviceResponsesInLoopAC object</returns>
        private async Task<InoviceResponsesInLoopAC> GetInvoicesOfGivenPageAsync(HttpClient httpClient, SearchInvoicesRequestAC searchInvoicesRequest, string accessToken, int pageNumber)
        {
            //Create request.
            using var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), string.Format("{0}{1}&{2}&{3}",
                    _configuration.GetValue<string>("PayPal:SearchInvoicesApi"), string.Format(StringConstant.HttpRequestUrlParameterValue, StringConstant.PayPalInvoiceTotalRequired, "true"),
                    string.Format(StringConstant.HttpRequestUrlParameterValue, StringConstant.PayPalInvoicePage, pageNumber),
                    string.Format(StringConstant.HttpRequestUrlParameterValue, StringConstant.PayPalInvoicePageSize, 100)));

            //Add authorization parameters in header.
            request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));

            //Serialize the request object with required naming strategy.
            var requestJson = JsonConvert.SerializeObject(searchInvoicesRequest, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                Formatting = Formatting.Indented
            });
            request.Content = new StringContent(requestJson);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);

            //Send request and check the status of it.
            var response = await httpClient.SendAsync(request);

            //Check if the request is successfully made.
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<InoviceResponsesInLoopAC>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                    Formatting = Formatting.Indented
                });
            }
            else
            {
                throw new HttpRequestException(StringConstant.PayPalApiRequestError);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Method to get invoices from PayPal.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to generate access token</param>
        /// <param name="startDate">Start date of given period</param>
        /// <param name="endDate">End date of given period</param>
        /// <returns>InvoicesResponseJsonAC object</returns>
        public async Task<InvoicesResponseJsonAC> GetPayPalInvoicesAsync(string authorizationCode, string startDate, string endDate)
        {
            //Get the access token.
            var accessToken = await GetPayPalAccessTokenAsync(authorizationCode);

            //Create HttpClient.
            using var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });

            //Create search request object and assign values.
            SearchInvoicesRequestAC searchInvoicesRequest = new SearchInvoicesRequestAC
            {
                InvoiceDateRange = new InvoiceDateRangeAC
                {
                    Start = startDate,
                    End = endDate
                }
            };

            int pageNumber = 1;
            InoviceResponsesInLoopAC response = await GetInvoicesOfGivenPageAsync(httpClient, searchInvoicesRequest, accessToken, pageNumber);
            ++pageNumber;

            while (pageNumber <= response.TotalPages)
            {
                var t = (await GetInvoicesOfGivenPageAsync(httpClient, searchInvoicesRequest, accessToken, pageNumber));
                response.Items.AddRange(t.Items);
                ++pageNumber;
            }

            //Serialize the complete object after all the loop calls.
            var responseJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                Formatting = Formatting.Indented
            });

            //Deserialize the response into the required return type object.
            InvoicesResponseJsonAC responseJsonAC = new InvoicesResponseJsonAC
            {
                InvoicesResponseJson = responseJson,
                InvoicesResponse = JsonConvert.DeserializeObject<InvoicesResponseAC>(responseJson, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                    Formatting = Formatting.Indented
                })
            };
            return responseJsonAC;
        }
        #endregion
    }
}
