using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class StripeUtility : IStripeUtility
    {
        #region Private variables
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public StripeUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Private method(s)
        /// <summary>
        /// Method to get access token as api request's user id from authorization code.
        /// </summary>
        /// <param name="authorizationCode">Authorization code</param>
        /// <returns>Access token string</returns>
        private async Task<string> GetStripeAccessTokenAsApiRequestUserIdAsync(string authorizationCode)
        {
            //Set the required parameters' values for token generation.
            StripeConfiguration.ApiKey = _configuration.GetValue<string>("Stripe:SecretAPIKey");
            var options = new OAuthTokenCreateOptions
            {
                GrantType = StringConstant.HttpGrantTypeAuthorizationCode,
                Code = authorizationCode
            };

            var service = new OAuthTokenService();
            var response = await service.CreateAsync(options);
            return response.StripeUserId;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Method to get invoices from Stripe.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to generate access token</param>
        /// <param name="startDate">Start date of given period</param>
        /// <param name="endDate">End date of given period</param>
        /// <returns>StripeList<Invoice> object</returns>
        public async Task<StripeList<Invoice>> GetStripeInvoicesAsync(string authorizationCode, DateTime startDate, DateTime endDate)
        {
            //Get the user id (access token).
            var userId = await GetStripeAccessTokenAsApiRequestUserIdAsync(authorizationCode);
            var requestOptions = new RequestOptions
            {
                StripeAccount = userId
            };

            //Set the required parameters' values to fetch the invoices.
            var options = new InvoiceListOptions
            {
                Limit = 100,
                Created = new DateRangeOptions
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate,
                },
            };

            //Send request to fetch the invoices.
            var service = new InvoiceService();
            StripeList<Invoice> invoices = await service.ListAsync(options, requestOptions);

            bool hasMore = invoices.HasMore;
            string startingAfterId = invoices.Data.Any() ? invoices.Data[invoices.Data.Count - 1].Id : null;
            while (hasMore)
            {
                //Set the required parameters' values to fetch the invoices.
                var optionsWithStartingAfter = new InvoiceListOptions
                {
                    Limit = 100,
                    Created = new DateRangeOptions
                    {
                        GreaterThanOrEqual = startDate,
                        LessThanOrEqual = endDate,
                    },
                    StartingAfter = startingAfterId
                };

                //Send request to fetch the invoices.
                StripeList<Invoice> tempInvoices = await service.ListAsync(optionsWithStartingAfter);

                //Add the fetched invoices to firstly created object. 
                invoices.Data.AddRange(tempInvoices.Data);

                //Set the required parameters' values to decide whether to call API again or not. 
                hasMore = tempInvoices.HasMore;
                startingAfterId = tempInvoices.Data[invoices.Data.Count - 1].Id;
            }

            return invoices;
        }
        #endregion
    }
}
