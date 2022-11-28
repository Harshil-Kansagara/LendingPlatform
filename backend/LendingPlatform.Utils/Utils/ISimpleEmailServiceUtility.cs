using LendingPlatform.Utils.ApplicationClass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface ISimpleEmailServiceUtility
    {
        /// <summary>
        /// Method creates and sends an email to shareholders.
        /// </summary>
        /// <param name="emailLoanDetails">List of EmailLoanDetailsAC object</param>
        /// <returns></returns>
        Task SendEmailToShareHoldersAsync(List<EmailLoanDetailsAC> emailLoanDetails);
    }
}
