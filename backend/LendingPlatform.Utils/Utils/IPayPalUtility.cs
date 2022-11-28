using LendingPlatform.Utils.ApplicationClass.PayPal;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IPayPalUtility
    {
        /// <summary>
        /// Method to get invoices from PayPal.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to generate access token</param>
        /// <param name="startDate">Start date of given period</param>
        /// <param name="endDate">End date of given period</param>
        /// <returns>InvoicesResponseJsonAC object</returns>
        Task<InvoicesResponseJsonAC> GetPayPalInvoicesAsync(string authorizationCode, string startDate, string endDate);
    }
}
