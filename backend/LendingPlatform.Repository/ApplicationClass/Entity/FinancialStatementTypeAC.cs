using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class FinancialStatementTypeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the financial statement type object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the financial statement type
        /// </summary>
        public string Name { get; set; }
        #endregion
    }
}
