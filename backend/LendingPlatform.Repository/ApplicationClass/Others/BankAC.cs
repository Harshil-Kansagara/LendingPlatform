using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class BankAC
    {
        #region Public properties
        /// <summary>
        /// Unique identifier for Bank.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the bank.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// SWIFT code of the bank.
        /// </summary>
        public string SWIFTCode { get; set; }
        #endregion
    }
}
