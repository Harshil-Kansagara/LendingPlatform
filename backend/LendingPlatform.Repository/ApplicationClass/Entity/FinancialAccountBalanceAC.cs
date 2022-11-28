using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class FinancialAccountBalanceAC
    {
        #region Public Properties
        /// <summary>
        /// Total Amount
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Name of financial account balance
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Period for which the finance account is retrieved
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Identifier for financial account
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Parent Id for financial account
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Expected/common value for approving loan
        /// </summary>
        public decimal? ExpectedValue { get; set; }

        /// <summary>
        /// Order of the account
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// List of account and amount mapped from Source
        /// </summary>
        public List<AccountChartAC> Source { get; set; }

        /// <summary>
        /// Json of Source accounts
        /// </summary>
        public string SourceJson { get; set; }
        #endregion
    }
}
