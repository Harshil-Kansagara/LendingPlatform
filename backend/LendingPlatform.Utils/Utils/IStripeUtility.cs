using Stripe;
using System;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IStripeUtility
    {
        /// <summary>
        /// Method to get invoices from Stripe.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to generate access token</param>
        /// <param name="startDate">Start date of given period</param>
        /// <param name="endDate">End date of given period</param>
        /// <returns>StripeList<Invoice> object</returns>
        Task<StripeList<Invoice>> GetStripeInvoicesAsync(string authorizationCode, DateTime startDate, DateTime endDate);
    }
}
