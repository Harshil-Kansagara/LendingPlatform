using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class PagedLoanApplicationsAC
    {
        /// <summary>
        /// Count of total applications.
        /// </summary>
        public int TotalApplicationsCount { get; set; }
        /// <summary>
        /// List of applications (after paging/filetring/sorting).
        /// </summary>
        public List<ApplicationAC> Applications { get; set; }

        /// <summary>
        /// Whether loan delete is allowed
        /// </summary>
        public bool IsDeleteAuthorized { get; set; }
    }
}
