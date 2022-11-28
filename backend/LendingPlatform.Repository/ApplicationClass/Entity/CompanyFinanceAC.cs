using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class CompanyFinanceAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for finance object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Financial Statement like Income Statement, Balance Sheet
        /// </summary>
        public string FinancialStatement { get; set; }

        /// <summary>
        /// Name of third party service
        /// </summary>
        public string ThirdPartyServiceName { get; set; }

        /// <summary>
        /// List of Financial account details (Periodical list of each standard account and amount)
        /// </summary>
        public List<PeriodicFinancialAccountsAC> FinancialAccounts { get; set; }
        /// <summary>
        /// JSON string financial information
        /// </summary>
        public string FinancialJson { get; set; }

        /// <summary>
        /// Flag to indicate if background lambda has mapped all finances or not (used during polling)
        /// </summary>
        public bool IsChartOfAccountMapped { get; set; }

        /// <summary>
        /// List of mapped Standard account and list of amount (Each account with list of amount)
        /// </summary>
        public List<StandardAccountsAC> StandardAccountList { get; set; }

        /// <summary>
        /// Updated date time
        /// </summary>
        public DateTime? UpdatedOn { get; set; }

        /// <summary>
        /// The number by which the amount is divided (default thousands)
        /// </summary>
        public decimal DivisionFactor { get; set; }

        /// <summary>
        /// End period till which the finance report is for
        /// </summary>
        public string EndPeriod { get; set; }

        /// <summary>
        /// Datetime when finances were retrieved
        /// </summary>
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Company selected in third party connected service
        /// </summary>
        public string ThirdPartyWiseCompanyName { get; set; }

        /// <summary>
        /// Check if the company has no data for any year in third party connected services
        /// </summary>
        public bool IsDataEmpty { get; set; }
        #endregion
    }
}
