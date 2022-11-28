using LendingPlatform.Repository.ApplicationClass.Entity;
using System;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class LoanEntityBankDetailsAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier of the loan application for which bank details are to be saved.
        /// </summary>
        public Guid LoanApplicationId { get; set; }
        /// <summary>
        /// Bank details of the account in which loan amount will be transferred.
        /// </summary>
        public EntityBankDetailsAC LoanAmountDepositeeBank { get; set; }
        /// <summary>
        /// Bank details of the account from which EMI will be deducted.
        /// </summary>
        public EntityBankDetailsAC EMIDeducteeBank { get; set; }
        #endregion
    }
}
