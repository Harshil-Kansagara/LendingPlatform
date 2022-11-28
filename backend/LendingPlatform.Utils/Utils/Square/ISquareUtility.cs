using Square.Models;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface ISquareUtility
    {
        /// <summary>
        /// Fetch invoices from square API
        /// </summary>
        /// <param name="authorizaionCode"></param>
        /// <returns></returns>
        Task<SearchInvoicesResponse> GetSquareInvoicesAsync(string authorizaionCode);
    }
}