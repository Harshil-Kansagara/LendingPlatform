using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceSummaryAC
    {
        /// <summary>
        /// List of accounts containing categories' details
        /// </summary>
        public List<PersonalFinanceAccountSummaryAC> Accounts { get; set; }
    }
}
