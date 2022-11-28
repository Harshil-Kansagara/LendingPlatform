using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class BalanceSheetAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for financial statement
        /// </summary>
        public Guid FinancialStatementId { get; set; }
        /// <summary>
        /// JSON string of financial information
        /// </summary>
        public string FinancialInformationJson { get; set; }
        #endregion
    }
}
