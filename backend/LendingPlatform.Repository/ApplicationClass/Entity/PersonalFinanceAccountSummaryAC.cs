using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAccountSummaryAC
    {
        /// <summary>
        /// Account name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// List of categories linked with account
        /// </summary>
        public List<PersonalFinanceCategorySummaryAC> Categories { get; set; }
    }
}
