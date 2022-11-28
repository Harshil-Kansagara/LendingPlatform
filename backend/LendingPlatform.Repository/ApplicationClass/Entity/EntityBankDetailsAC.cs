using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class EntityBankDetailsAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the Bank.
        /// </summary>
        public Guid BankId { get; set; }
        /// <summary>
        /// Entity's account number.
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Name of the bank.
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// SWIFT code of the bank.
        /// </summary>
        public string SWIFTCode { get; set; }
        #endregion
    }
}
