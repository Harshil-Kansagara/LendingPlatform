using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PeriodicFinancialAccountsAC
    {
        #region Public Properties
        /// <summary>
        /// Time period of financial account
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// List of financial account balance deatails
        /// </summary>
        public List<FinancialAccountBalanceAC> FinancialAccountBalances { get; set; }

        /// <summary>
        /// Name of financial report ex, Income Statement or Balance Sheet etc
        /// </summary>
        public string ReportName { get; set; }

        /// <summary>
        /// Flag gets true when xero service is connected
        /// </summary>
        public bool IsXero { get; set; }
        #endregion
    }
}
