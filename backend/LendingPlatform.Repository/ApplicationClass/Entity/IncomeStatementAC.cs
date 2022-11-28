using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class IncomeStatementAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for financial statement object
        /// </summary>
        public Guid FinancialStatementId { get; set; }
        /// <summary>
        /// JSON string of the financial information
        /// </summary>
        public string FinancialInformationJson { get; set; }
        #endregion
    }
}
