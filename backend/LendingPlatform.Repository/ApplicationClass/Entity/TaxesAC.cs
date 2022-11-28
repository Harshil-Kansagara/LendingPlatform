using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class TaxesAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for financial statement object
        /// </summary>
        public Guid FinancialStatementId { get; set; }
        /// <summary>
        /// Financial statement
        /// </summary>
        public string FinancialStatement { get; set; }

        #endregion
    }
}
