using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Square;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class SquareUtility : ISquareUtility
    {
        #region Private Variables

        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        #endregion

        #region Constructor
        public SquareUtility(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Public method(s)
        /// <summary>
        /// Fetch invoices from square API
        /// </summary>
        /// <param name="authorizaionCode"></param>
        /// <returns>Return JSON string</returns>
        public async Task<SearchInvoicesResponse> GetSquareInvoicesAsync(string authorizaionCode)
        {
            /*Square SDK(https://github.com/square/square-dotnet-sdk/) will be used to fetch invoices from square.
             Two SDK API will be used(Obtain Token, Search Invoices)
             Obtain token API is for obtaining access token with the help of authorization code*/

            //Set up SquareClient with access token and envirnoment as sandbox
            SquareClient client = new SquareClient.Builder()
                .Environment(_configuration.GetValue<Square.Environment>("Square:Environment"))
                .AccessToken(_configuration.GetValue<string>("Square:AccessToken"))
                .Build();

            Square.Apis.IOAuthApi oAuthApi = client.OAuthApi;

            //Initialiaze body with authorization code, clientId and Client Secret
            ObtainTokenRequest obtainTokenBody = new ObtainTokenRequest.Builder(
            clientId: _configuration.GetValue<string>("Square:ClientId"),
            clientSecret: _configuration.GetValue<string>("Square:ClientSecret"),
            grantType: StringConstant.HttpGrantTypeAuthorizationCode)
              .Code(authorizaionCode)
              .Build();

            //Call square obtain token API 
            ObtainTokenResponse obtainTokenResponse = await oAuthApi.ObtainTokenAsync(obtainTokenBody);

            //Use obtained access token to again Set up SquareClient with access token and envirnoment as sandbox for search invoices API
            client = new SquareClient.Builder()
            .Environment(_configuration.GetValue<Square.Environment>("Square:Environment"))
            .AccessToken(obtainTokenResponse.AccessToken)
            .Build();

            //Api to fetch LocationIds of the merchant
            Square.Apis.ILocationsApi locationsApi = client.LocationsApi;
            ListLocationsResponse listLocationsResponse = await locationsApi.ListLocationsAsync();

            //Initialiaze body with locationIds, InvoiceSort and limi of 200
            List<string> locationIds = new List<string>
            {
                //First locationId is selected from the list of the locationIds
                listLocationsResponse.Locations[0].Id
            };

            InvoiceFilter filter = new InvoiceFilter.Builder(locationIds: locationIds)
              .Build();

            InvoiceSort sort = new InvoiceSort.Builder(field: StringConstant.SquareInvoiceSortDate)
              .Order(StringConstant.DescendingOrderShorthand)
              .Build();

            InvoiceQuery query = new InvoiceQuery.Builder(filter: filter)
              .Sort(sort)
              .Build();

            SearchInvoicesRequest searchInvoicesBody = new SearchInvoicesRequest.Builder(query: query)
              .Limit(200)
              .Build();

            List<Invoice> invoiceList = new List<Invoice>();

            //Call square search invoices API 
            SearchInvoicesResponse searchInvoicesResponse = await client.InvoicesApi.SearchInvoicesAsync(body: searchInvoicesBody);

            string cursor = searchInvoicesResponse.Cursor;

            if (searchInvoicesResponse.Invoices != null)
            {
                invoiceList.AddRange(searchInvoicesResponse.Invoices);

                //Select previous nth year.
                var lastNYears = DateTime.UtcNow.AddYears(-_configuration.GetValue<int>("FinancialYear:Years"));

                //Set start and end date.
                DateTime startDate = new DateTime(lastNYears.Year, DateTime.ParseExact(_configuration.GetValue<string>("FinancialYear:StartMonth"), "MMMM", CultureInfo.InvariantCulture).Month, 01);
                DateTime endDate = DateTime.UtcNow;


                //Check if response has cursor(pagination) field, if yes then again send request
                while (cursor != null)
                {
                    SearchInvoicesRequest searchInvoicesPaginationBody = new SearchInvoicesRequest.Builder(query: query)
                      .Limit(200)
                      .Cursor(cursor)
                      .Build();

                    SearchInvoicesResponse searchInvoicesResponsePagination = await client.InvoicesApi.SearchInvoicesAsync(body: searchInvoicesPaginationBody);

                    cursor = searchInvoicesResponsePagination.Cursor;

                    //Add up all invoices and return SearchInvoicesResponse object.
                    if (searchInvoicesResponsePagination.Invoices != null)
                    {
                        invoiceList.AddRange(searchInvoicesResponsePagination.Invoices);
                        if (searchInvoicesResponsePagination.Invoices.Any(x => (DateTime.Parse(x.CreatedAt).ToUniversalTime() <= startDate)))
                        {
                            cursor = null;
                        }
                    }
                }

                invoiceList = invoiceList.Where(x => (DateTime.Parse(x.CreatedAt).ToUniversalTime() >= startDate) && (DateTime.Parse(x.CreatedAt).ToUniversalTime() <= endDate)).OrderByDescending(o => o.CreatedAt).ToList();
            }

            SearchInvoicesResponse searchInvoicesResponseWithAllInvoices = new SearchInvoicesResponse(invoiceList, searchInvoicesResponse.Cursor, searchInvoicesResponse.Errors);

            return searchInvoicesResponseWithAllInvoices;
        }
        #endregion 
    }
}
